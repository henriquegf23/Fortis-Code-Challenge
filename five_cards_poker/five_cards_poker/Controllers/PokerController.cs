using five_cards_poker.Models;
using five_cards_poker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace five_cards_poker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PokerController : ControllerBase
    {
        private readonly IPokerService _pokerService;

        private readonly ILogger<PokerController> _logger;

        public PokerController(ILogger<PokerController> logger, IPokerService pokerService)
        {
            _logger = logger;
            _pokerService = pokerService;
        }


        /// <summary>
        /// Simulates a round of poker
        /// </summary>
        /// <remarks>
        /// This endpoint is responsible for generating a full round of poker using the number of players specified by the caller, with a minimum of 1 and a maximum of 10 players.
        /// The "shuffleSeed" parameter is used to shuffle the deck. If blank, the seed is generated automatically. This parameter can be used to reproduce the same game for debugging purposes.
        /// 
        /// This implementation does not necessarily use all the "action endpoints" to generate a game because, since all parameters are controlled internally, it reduces potential bugs from user input and sometimes uses fewer loops to process information.
        /// </remarks>
        [HttpGet(Name = "GetPlayPoker")]
        public ActionResult<Table> Get(int numOfPlayers, int? shuffleSeed)
        {

            if (numOfPlayers < 1 || numOfPlayers > 10)
            {
                return BadRequest("Number of players must be between 1 and 10.");
            }

            try
            {
                Table result = _pokerService.PlayPoker(numOfPlayers, shuffleSeed);
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Error - ." + ex.Message);
            }

        }

    }
}
