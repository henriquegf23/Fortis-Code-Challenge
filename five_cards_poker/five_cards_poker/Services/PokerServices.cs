using five_cards_poker.Models;
using five_cards_poker.Models.DTOs;
using five_cards_poker.Models.Interfaces;
using five_cards_poker.Repository.Interfaces;
using five_cards_poker.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace five_cards_poker.Services
{
    public class PokerServices : IPokerService
    {
        private readonly IDeckRepository _deckRepository;
        private readonly IDeckServices _deckServices;
        private readonly IMappingDTOServices _mappingDTOServices;

        public Deck deck { get; private set; }

        public const int maxCardOnHand = 5;

        public PokerServices(IDeckRepository deckRepository, IDeckServices deckServices, IMappingDTOServices mappingDTOServices) 
        {
            _deckRepository = deckRepository;
            _deckServices = deckServices;
            _mappingDTOServices = mappingDTOServices;

            deck = new Deck(_deckRepository);
        }

        public Table PlayPoker(int numOfPlayers, int? shuffleSeed)
        {
            deck = ShuffleDeck(deck, shuffleSeed);

            List<PlayerDTO> tablePlayers = new List<PlayerDTO>();
            for(int i = 0; i < numOfPlayers; i++)
            {
                List<Card> hand = GiveCardsToPlayers(deck, maxCardOnHand);

                HandDetails hd = EvaluateHand(hand);
                Score score = TranslateScore(hd);

                Player p = new Player
                {
                    Id = i + 1,
                    Hand = hand,
                    HandDetails = hd,
                    Score = score
                };

                tablePlayers.Add(ConvertPlayerToDTO(p));
            }

            Table table = GetTableResult(tablePlayers);

            return table;
        }

        public Deck ShuffleDeck(Deck deck, int? shuffleSeed)
        {
            return _deckServices.ShuffleCards(deck, shuffleSeed);
        }

        public List<Card> GiveCardsToPlayers(Deck deck, int maxCardOnHand)
        {
            List<Card> hand = new List<Card>();
            for (int j = 0; j < maxCardOnHand; j++)
            {
                hand.Add(deck.cards.Last());
                deck.cards.RemoveAt(deck.cards.Count - 1);
            }

            return hand.OrderBy(x => x.Value).ToList();
        }

        public HandDetails EvaluateHand(List<Card> playersHand)
        {
            Dictionary<string, int> validateSuits = new Dictionary<string, int>();
            int isThereAces = 0;
            Dictionary<int, int> validateEquals = new Dictionary<int, int>();

            bool isInOrder = true;
            int sumOrder = 0;
            int hightestCard = 0;

            int firstValue = 0;
            int aux = 0;

            for (int i = 0; i < playersHand.Count; i++)
            {
                //Verify suits...
                string cardSuit = playersHand[i].Suit;
                Card playerCard = playersHand[i];

                if (validateSuits.ContainsKey(cardSuit))
                {
                    validateSuits[cardSuit]++;
                }
                else
                {
                    validateSuits.Add(cardSuit, 1);
                }

                //Verify if there is Aces...
                if (playerCard.Value == 14)
                    isThereAces++;

                //Verify Equal value cards...
                if (validateEquals.ContainsKey(playerCard.Value))
                {
                    validateEquals[playerCard.Value]++;
                }
                else
                {
                    validateEquals.Add(playerCard.Value, 1);
                }

                //Verify Order
                int differentRankCards = playersHand.GroupBy(x => x.Value).Count();
                if (differentRankCards == 5)
                {
                    if (isInOrder)
                    {
                        if (i == 0)
                        {
                            firstValue = playerCard.Value;
                            aux = playerCard.Value;
                        }
                        else
                        {
                            if (playerCard.Value == aux + 1)
                            {
                                aux = playerCard.Value;
                            }
                            else if (playerCard.AlternativeValue != null && playerCard.AlternativeValue == firstValue - 1)
                            {
                                isInOrder = true;
                            }
                            else
                            {
                                isInOrder = false;
                            }
                        }
                    }
                }
                else
                {
                    isInOrder = false;
                }
            }

            //Sum order
            if (isInOrder)
            {
                sumOrder = playersHand.Sum(p => p.Value);
            }

            //Highest card
            hightestCard = playersHand.LastOrDefault().Value;


            HandDetails handDetail = new HandDetails { 
               ValidateSuits = validateSuits,
               IsThereAces = isThereAces,                    
               ValidateEquals = validateEquals,
               IsInOrder = isInOrder,
               SumOrder = sumOrder,
               HighestCard = hightestCard
            };

            return handDetail;
        }

        public Score TranslateScore(HandDetails handDetails)
        {
            Score score = new Score();

            score.tiebreaker = new List<int>();

            int highestCardRank = handDetails.HighestCard;

            if (handDetails.IsInOrder && handDetails.ValidateSuits.Count() == 1)
            {
                if (handDetails.SumOrder == 60)
                {
                    // rank 1
                    score.Rank = 1;
                    score.Description = "Royal Flush";
                    score.tiebreaker = null;
                }
                else
                {
                    // rank 2
                    score.Rank = 2;
                    score.Description = "Straight flush";
                    score.tiebreaker.Add(highestCardRank);
                }
            }else if (handDetails.IsInOrder)
            {
                //rank 6
                score.Rank = 6;
                score.Description = "Straight";
                score.tiebreaker.Add(highestCardRank);
            }
            else if(handDetails.ValidateSuits.Count() == 1)
            {
                //rank 5
                score.Rank = 5;
                score.Description = "Flush";
                score.tiebreaker.Add(highestCardRank);
            }
            else
            {
                int minEqualCards = handDetails.ValidateEquals.Min(x => x.Value);
                int minEqualCardsRank = handDetails.ValidateEquals.Where(x => x.Value == minEqualCards).Min(x => x.Key);

                int maxEqualCards = handDetails.ValidateEquals.Max(x => x.Value);
                int maxEqualCardsRank = handDetails.ValidateEquals.Where(x => x.Value == maxEqualCards).Max(x => x.Key);

                int countCardsGroups = handDetails.ValidateEquals.Count();

                if (maxEqualCards == 4)
                {
                    //rank 3
                    score.Rank = 3;
                    score.Description = "Four of a Kind";
                    score.tiebreaker.Add(maxEqualCardsRank);
                }
                else if (maxEqualCards == 3 && minEqualCards == 2)
                {
                    //rank 4
                    score.Rank = 4;
                    score.Description = "Full House";
                    score.tiebreaker.Add(maxEqualCardsRank);
                }
                else if (maxEqualCards == 3 && minEqualCards == 1)
                {
                    //rank 7
                    score.Rank = 7;
                    score.Description = "Three of a Kind";
                    score.tiebreaker.Add(maxEqualCardsRank);
                }
                else
                {
                    int verifyPairs = handDetails.ValidateEquals.Count(e => e.Value == 2);
                    if (verifyPairs == 2) 
                    {
                        //rank 8
                        score.Rank = 8;
                        score.Description = "Two Pair";
                        List<int> pairOrder = handDetails.ValidateEquals.Where(x => x.Value == 2)
                                                                        .OrderByDescending(x => x.Key)
                                                                        .Select(x => x.Key)
                                                                        .ToList();
                        
                        score.tiebreaker.AddRange(pairOrder);
                        int addLastTiebreakerCardOnTwoPairs = handDetails.ValidateEquals.Where(x => x.Value == 1).Select(x => x.Key).FirstOrDefault();

                        score.tiebreaker.Add(addLastTiebreakerCardOnTwoPairs);
                    }
                    else if(verifyPairs == 1)
                    {
                        //rank 9
                        score.Rank = 9;
                        score.Description = "Pair";
                        score.tiebreaker.Add(maxEqualCardsRank);

                        List<int> highestOrderCardOutOfGroup = GetCardsOutOfEqualsGroupDesc(handDetails, maxEqualCardsRank);
                        score.tiebreaker.AddRange(highestOrderCardOutOfGroup);
                    }
                    else
                    {
                        //rank 10
                        score.Rank = 10;
                        score.Description = $"High Card {highestCardRank}";
                        List<int> cardsOrder = handDetails.ValidateEquals.OrderByDescending(x => x.Key)
                                                                         .Select(x => x.Key)
                                                                         .ToList();
                        score.tiebreaker.AddRange(cardsOrder);
                    }

                }
            }

            return score;
        }

        public Table GetTableResult(List<PlayerDTO> tablePlayers)
        {
            Table tableResult = new Table();

            tableResult.Players = tablePlayers;

            int lowestScoreRank = tablePlayers.Min(x => x.Score.Rank);

            List<PlayerDTO> lowestScoreRankPlayers = tablePlayers.Where(x => x.Score.Rank == lowestScoreRank).ToList();

            List<PlayerDTO> winnerList = new List<PlayerDTO>();
            PlayerDTO possibleWinner = lowestScoreRankPlayers.First();
            int lowestScoreRankPlayersCount = lowestScoreRankPlayers.Count();
            string finalResult;

            bool draw = false;
            List<PlayerDTO> drawPlayers = new List<PlayerDTO>();

            if (lowestScoreRankPlayersCount == 1)
            {
                draw = false;
            }
            else
            {
                if(possibleWinner.Score.Rank == 1)
                {
                    //Draw Royal Flush
                    drawPlayers.AddRange(lowestScoreRankPlayers);
                    draw = true;
                }

                int tiebreakerCount = possibleWinner.Score.tiebreaker.Count;
                int sumAllCards = possibleWinner.Score.tiebreaker.Sum();

                for (int i = 1; i < lowestScoreRankPlayersCount; i++)
                {
                    
                    for(int j = 0; j < tiebreakerCount; j++)
                    {
                        if (possibleWinner.Score.tiebreaker[j] < lowestScoreRankPlayers[i].Score.tiebreaker[j])
                        {
                            possibleWinner = lowestScoreRankPlayers[i];
                            draw = false;
                            break;
                        }
                        else if(possibleWinner.Score.tiebreaker[j] > lowestScoreRankPlayers[i].Score.tiebreaker[j])
                        {
                            draw = false;
                            break;
                        }
                        else
                        {
                            draw = true;
                        }
                    }

                    if (draw)
                    {
                        if(drawPlayers.Count == 0)
                        {
                            drawPlayers.Add(possibleWinner);
                        }
                        drawPlayers.Add(lowestScoreRankPlayers[i]);
                        
                    }
                    
                }
            }

            if (draw)
            {
                finalResult = "Draw - " + drawPlayers[0].Score.Description;
                winnerList.AddRange(drawPlayers);
            }
            else
            {
                finalResult = possibleWinner.Score.Description;
                winnerList.Add(possibleWinner);
            }

            tableResult.Winners = winnerList;
            tableResult.FinalResult = finalResult;
            return tableResult;
        }

        public List<int> GetCardsOutOfEqualsGroupDesc(HandDetails hd, int maxEqualCardsRank)
        {
            return hd.ValidateEquals.Where(x => x.Key != maxEqualCardsRank)
                                    .OrderByDescending(x => x.Key)
                                    .Select(x => x.Key)
                                    .ToList();
        }

        public PlayerDTO ConvertPlayerToDTO(Player p)
        {
            return _mappingDTOServices.ConvertPlayerToDTO(p);
        }
    }
}
