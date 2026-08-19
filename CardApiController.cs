using Microsoft.AspNetCore.Mvc;
using CCBInteractiveApp.Services;

namespace CCBInteractiveApp.Controllers
{
    [ApiController]
    [Route("api/card")]
    public class CardApiController : ControllerBase
    {
        private readonly CardService _cardService;
        public CardApiController(CardService cardService)
        {
            _cardService = cardService;
        }

        [HttpPost("add-question")]
        public IActionResult AddQuestion([FromBody] AddQuestionReq req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Question))
                return BadRequest(new { success = false, message = "问题内容不能为空" });

            _cardService.AddQuestion(req);
            return Ok(new { success = true });
        }

        [HttpGet("get-random-card")]
        public IActionResult GetRandomCard(string targetSeniority, string currentEmpId)
        {
            if (string.IsNullOrEmpty(targetSeniority) || string.IsNullOrEmpty(currentEmpId))
                return BadRequest(new { success = false, message = "缺少必要参数" });

            var card = _cardService.GetRandomQuestion(targetSeniority, currentEmpId);
            return Ok(new { success = true, card });
        }

        [HttpPost("submit-answer")]
        public IActionResult SubmitAnswer([FromBody] SubmitAnswerReq req)
        {
            if (req == null || req.QuestionId <= 0 || string.IsNullOrWhiteSpace(req.Answer))
                return BadRequest(new { success = false, message = "回答内容或问题ID无效" });

            var code = _cardService.SubmitAnswer(req);
            return Ok(new { success = true, code });
        }

        // 统一数据接口：供主界面底部面板和管理后台共用
        [HttpGet("admin-data")]
        public IActionResult GetAdminData()
        {
            var data = _cardService.GetAdminOverview();
            return Ok(data);
        }
    }
}
