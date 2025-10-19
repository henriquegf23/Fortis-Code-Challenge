using five_cards_poker.Models;
using five_cards_poker.Models.DTOs;
using five_cards_poker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace five_cards_poker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PokerActionsController : ControllerBase
    {
        private readonly IPokerActionsService _pokerActionsService;

        private readonly ILogger<PokerController> _logger;

        public PokerActionsController(ILogger<PokerController> logger, IPokerActionsService pokerActionsService)
        {
            _logger = logger;
            _pokerActionsService = pokerActionsService;
        }

        /// <summary>
        /// Generates poker hands
        /// </summary>
        /// <remarks>
        /// This endpoint is responsible for generating N 5-card poker hands for up to 10 players.
        /// The "shuffleSeed" parameter is used to shuffle the deck. If blank, the seed is generated automatically. This parameter can be used to reproduce the same game for debugging purposes.
        /// 
        /// The response of this call will be a list of lists of cards. Each card has the following attributes: Value (numeric value), Name (rank), AlternativeValue (used for Aces in lower sequences), and Suit.
        /// </remarks>
        [HttpGet("GetHand")]
        public ActionResult<List<List<Card>>> GetHand(int numOfPlayers, int? shuffleSeed)
        {

            if (numOfPlayers < 1 || numOfPlayers > 10)
            {
                return BadRequest("Number of players must be between 1 and 10.");
            }

            try
            {
                List<List<Card>> result = _pokerActionsService.GiveCardsToPlayers(numOfPlayers, shuffleSeed);
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Error - ." + ex.Message);
            }

        }

        /// <summary>
        /// Validates a poker hand
        /// </summary>
        /// <remarks>
        /// Verifies if all 5 cards are valid in a standard 52-card deck poker game.
        /// The endpoint will return a structure with an IsValid boolean field and a message.
        /// 
        /// Validations: More or less than 5 cards, invalid suit, invalid rank (2-14, where 14 represents Ace), and duplicate cards (same rank and suit).
        /// </remarks>
        [HttpPost("PostValidateHand")]
        public ActionResult<ValidHand> ValidateHand([FromBody] List<Card> hand)
        {
            try
            {
                ValidHand result = _pokerActionsService.ValidateHand(hand);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error validating hand - " + ex.Message);
            }
        }

        /// <summary>
        /// Validates multiple poker hands
        /// </summary>
        /// <remarks>
        /// Verifies if all 5-card hands for multiple players are valid in a standard 52-card deck poker game.
        /// The endpoint will return a structure with an IsValid boolean field and a message.
        /// 
        /// Validations: More or less than 5 cards in each player's hand, invalid suit, invalid rank (2-14, where 14 represents Ace), and duplicate cards (same rank and suit across all players).
        /// </remarks>
        [HttpPost("PostValidateMultipleHands")]
        public ActionResult<ValidHand> ValidateMultipleHands([FromBody] List<List<Card>> hands)
        {
            try
            {
                ValidHand result = _pokerActionsService.ValidateMultipleHands(hands);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error validating hand - " + ex.Message);
            }
        }

        /// <summary>
        /// Evaluates and translates the poker score of a hand
        /// </summary>
        /// <remarks>
        /// Validates and evaluates the score of a poker hand.
        /// 
        /// The endpoint will return a PlayerDTO structure containing an identifier (irrelevant for a single player), a simplified version of the player's cards, and the hand score along with tiebreaker information.
        /// </remarks>
        [HttpPost("PostEvaluateHandAndTranslateScore")]
        public ActionResult<PlayerDTO> EvaluateHandAndTranslateScore([FromBody] List<Card> hand)
        {
            try
            {
                PlayerDTO result = _pokerActionsService.EvaluateHandAndTranslateScore(hand);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error - " + ex.Message);
            }
        }

        /// <summary>
        /// Evaluates and translates the poker scores of multiple hands
        /// </summary>
        /// <remarks>
        /// Validates and evaluates the scores of multiple poker hands.
        /// 
        /// The endpoint will return a list of PlayerDTO structures, each containing an identifier, a simplified version of the player's cards, and the hand score along with tiebreaker information.
        /// </remarks>
        [HttpPost("PostEvaluateMultiHandAndTranslateScore")]
        public ActionResult<List<PlayerDTO>> EvaluateMultiHandsAndTranslateScore([FromBody] List<List<Card>> hand)
        {
            try
            {
                List<PlayerDTO> result = _pokerActionsService.EvaluateMultiHandsAndTranslateScore(hand);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error - " + ex.Message);
            }
        }

        /// <summary>
        /// Analyzes and determines the winner (or draw) of a poker round from a list of players
        /// </summary>
        /// <remarks>
        /// This endpoint receives a list of PlayerDTO objects. It compares all hands and determines a winner or draw for the round.
        /// 
        /// The return structure will be a Table object containing the list of players, the list of winners (more than one in case of a draw), and the winning score for that round.
        /// </remarks>
        [HttpPost("PostTableResultFromPlayersDTO")]
        public ActionResult<Table> ReturnTableResultFromPlayersDTO([FromBody] List<PlayerDTO> players)
        {
            try
            {
                Table result = _pokerActionsService.GetTableResult(players);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error - " + ex.Message);
            }
        }

        /// <summary>
        /// Analyzes and determines the winner (or draw) of a poker round from a list of cards
        /// </summary>
        /// <remarks>
        /// This endpoint receives a list of cards (generated from the endpoint GetHand). It compares all hands and determines a winner or draw for the round.
        /// 
        /// The return structure will be a Table object containing the list of players, the list of winners (more than one in case of a draw), and the winning score for that round.
        /// </remarks>
        [HttpPost("PostTableResultFromHand")]
        public ActionResult<Table> ReturnTableResultFromHand([FromBody] List<List<Card>> hands)
        {
            try
            {
                Table result = _pokerActionsService.ComparePlayersHands(hands);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error - " + ex.Message);
            }
        }

    }
}
