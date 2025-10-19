using five_cards_poker.Models;
using five_cards_poker.Models.DTOs;
using five_cards_poker.Models.Interfaces;
using five_cards_poker.Repository.Interfaces;
using five_cards_poker.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace five_cards_poker.Services
{
    public class PokerActionsServices : IPokerActionsService
    {
        private readonly IDeckRepository _deckRepository;
        private readonly IDeckServices _deckServices;
        private readonly IMappingDTOServices _mappingDTOServices;

        public Deck deck { get; private set; }

        public const int maxCardOnHand = 5;

        public PokerActionsServices(IDeckRepository deckRepository, IDeckServices deckServices, IMappingDTOServices mappingDTOServices) 
        {
            _deckRepository = deckRepository;
            _deckServices = deckServices;
            _mappingDTOServices = mappingDTOServices;

            deck = new Deck(_deckRepository);
        }

        public Deck ShuffleDeck(Deck deck, int? shuffleSeed)
        {
            return _deckServices.ShuffleCards(deck, shuffleSeed);
        }

        public List<List<Card>> GiveCardsToPlayers(int numOfPlayers, int? shuffleSeed)
        {
            List<List<Card>> handsRequested = new List<List<Card>>();

            ShuffleDeck(deck, shuffleSeed);

            for (int i = 0; i < numOfPlayers; i++)
            {
                List<Card> hand = new List<Card>();
                for (int j = 0; j < maxCardOnHand; j++)
                {
                    hand.Add(deck.cards.Last());
                    deck.cards.RemoveAt(deck.cards.Count - 1);
                }

                handsRequested.Add(hand.OrderBy(x => x.Value).ToList());
            }

            return handsRequested;
        }

        public ValidHand ValidateHand(List<Card> hand)
        {

            
            ValidHand validHand = new ValidHand();

            validHand.IsValid = true;
            validHand.Message = "Hand is valid";

            //Verify card count
            if (hand.Count != 5)
            {
                validHand.IsValid = false;
                validHand.Message = "Incorrect number of cards to be a valid hand";
                return validHand;
            }

            // Verify rank and suits
            foreach(Card c in hand)
            {
                validHand = ValidateRank(c.Value);
                if (!validHand.IsValid)
                {
                    return validHand;
                }

                validHand = ValidateSuit(c.Suit);
                if (!validHand.IsValid)
                {
                    return validHand;
                }
            }

            //Verify unique cards
            var uniqueCards = ValidadeUniqueCards(hand);

            if (uniqueCards.Count != 5)
            {
                validHand.IsValid = false;
                validHand.Message = "Multiple of same card on the hand";
                return validHand;
            }


            return validHand;
        }

        public ValidHand ValidateRank(int value)
        {
            ValidHand validHand = new ValidHand();

            validHand.IsValid = true;
            validHand.Message = "Hand is valid";

            if (value < 2 || value > 14)
            {
                validHand.IsValid = false;
                validHand.Message = "Invalid card rank";
                return validHand;
            }

            return validHand;
        }

        public ValidHand ValidateSuit(string suit)
        {
            ValidHand validHand = new ValidHand();

            validHand.IsValid = true;
            validHand.Message = "Hand is valid";

            List<string> suitList = new List<string> { "Hearts", "Diamonds", "Clubs", "Spades" };

            if (string.IsNullOrEmpty(suit))
            {
                validHand.IsValid = false;
                validHand.Message = "Suit should not be null";
                return validHand;
            }


            if (!suitList.Any(s => s.Equals(suit, StringComparison.OrdinalIgnoreCase)))
            {
                validHand.IsValid = false;
                validHand.Message = "Suit is not valid";
                return validHand;
            }

            return validHand;
        }

        public HashSet<string> ValidadeUniqueCards(List<Card> cards)
        {
            return cards.Select(c => $"{c.Value}-{c.Suit}").ToHashSet();
        }

        public ValidHand ValidateMultipleHands(List<List<Card>> hands)
        {
            ValidHand validHand = new ValidHand();

            int totalCardsInPlay = 0;
            List<Card> cardsInPlay = new List<Card>();
            foreach(List<Card> hand in hands)
            {
                totalCardsInPlay += hand.Count;
                cardsInPlay.AddRange(hand);
            }

            if(totalCardsInPlay > 50 || totalCardsInPlay % 5 != 0)
            {
                validHand.IsValid = false;
                validHand.Message = "Incorrect quantity of cards in play";
                return validHand;
            }

            var uniqueCards = ValidadeUniqueCards(cardsInPlay);

            if(uniqueCards.Count != totalCardsInPlay)
            {
                validHand.IsValid = false;
                validHand.Message = "There are equal cards in play";
                return validHand;
            }

            foreach(Card c in cardsInPlay)
            {
                validHand = ValidateRank(c.Value);
                if (!validHand.IsValid)
                {
                    return validHand;
                }

                validHand = ValidateSuit(c.Suit);
                if (!validHand.IsValid)
                {
                    return validHand;
                }

            }

            validHand.IsValid = true;
            validHand.Message = "All hands are valid";

            return validHand;
        }

        public PlayerDTO EvaluateHandAndTranslateScore(List<Card> hand)
        {
            PlayerDTO player = new PlayerDTO();
            List<CardDTO> handDTO = new List<CardDTO>();

            if(!hand.SequenceEqual(hand.OrderBy(h => h.Value)))
            {
                hand = hand.OrderBy(x => x.Value).ToList();
            }

            foreach(Card c in hand)
            {
                //Special case to avoid error on input for Aces
                if (c.Value == 14 && c.AlternativeValue != 1)
                    c.AlternativeValue = 1;

                handDTO.Add(ConvertCardToDTO(c));
            }
            player.Hand = handDTO;

            ValidHand vh = new ValidHand();

            vh = ValidateHand(hand);

            if(vh.IsValid == false)
            {
                throw new Exception("Hand is invalid");
            }

            HandDetails hd = new HandDetails();
            hd = EvaluateHand(hand);

            Score s = new Score();
            s = TranslateScore(hd);

            player.Score = s;

            return player;
        }
        public List<PlayerDTO> EvaluateMultiHandsAndTranslateScore(List<List<Card>> hands)
        {
            List<PlayerDTO> playersDTO = new List<PlayerDTO>();

            ValidHand vh = ValidateMultipleHands(hands);
            if (!vh.IsValid)
            {
                throw new Exception("Hand is invalid");
            }

            for (int i = 0; i < hands.Count; i++)
            {
                List<Card> currentPlayersCards = hands[i];

                if (!currentPlayersCards.SequenceEqual(currentPlayersCards.OrderBy(h => h.Value)))
                {
                    currentPlayersCards = currentPlayersCards.OrderBy(x => x.Value).ToList();
                }

                PlayerDTO p = new PlayerDTO();

                p.Id = i + 1;
                p.Hand = new List<CardDTO>();
                foreach (Card c in currentPlayersCards)
                {
                    //Special case to avoid error on input for Aces
                    if (c.Value == 14 && c.AlternativeValue != 1)
                        c.AlternativeValue = 1;

                    p.Hand.Add(ConvertCardToDTO(c));
                }

                HandDetails hd = EvaluateHand(currentPlayersCards);
                p.Score = TranslateScore(hd);

                playersDTO.Add(p);
            }

            return playersDTO;
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

        public Table ComparePlayersHands(List<List<Card>> playersHands)
        {
            Table table = new Table();
            List<PlayerDTO> playersDTO = new List<PlayerDTO>();

            playersDTO = EvaluateMultiHandsAndTranslateScore(playersHands);

            table = GetTableResult(playersDTO);

            return table;
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

        public CardDTO ConvertCardToDTO(Card c)
        {
            return _mappingDTOServices.ConvertCardToDTO(c);
        }
    }
}
