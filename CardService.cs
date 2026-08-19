using System;
using System.Collections.Generic;
using System.Linq;

namespace CCBInteractiveApp.Services
{
    public class QuestionItem
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmpId { get; set; }
        public string Seniority { get; set; }  // "new" 或 "veteran"
        public string Question { get; set; }
        public string AnimalIcon { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    public class MatchRecord
    {
        public int RecordId { get; set; }
        public int QuestionId { get; set; }
        public string AskName { get; set; }
        public string AskEmpId { get; set; }
        public string Question { get; set; }
        public string AnswerName { get; set; }
        public string AnswerEmpId { get; set; }
        public string Answer { get; set; }
        public string TicketCode { get; set; }
        public DateTime MatchTime { get; set; } = DateTime.Now;
    }

    // 请求 DTO
    public class AddQuestionReq
    {
        public string AuthorName { get; set; }
        public string AuthorEmpId { get; set; }
        public string Seniority { get; set; }
        public string Question { get; set; }
        public string AnimalIcon { get; set; }
    }

    public class SubmitAnswerReq
    {
        public int QuestionId { get; set; }
        public string AnswererName { get; set; }
        public string AnswererEmpId { get; set; }
        public string Answer { get; set; }
    }

    public class CardService
    {
        private readonly List<QuestionItem> _newbiePool = new List<QuestionItem>
        {
            new QuestionItem { Id = 1, AuthorName = "陈萌新", AuthorEmpId = "9001", Seniority = "new",
                Question = "刚入职遇到业务不熟悉被批评了，该怎么调整心态？", AnimalIcon = "🐣" },
            new QuestionItem { Id = 2, AuthorName = "李新人", AuthorEmpId = "9002", Seniority = "new",
                Question = "请问各位前辈，建投人必备的职业素养是什么？", AnimalIcon = "🐱" }
        };

        private readonly List<QuestionItem> _veteranPool = new List<QuestionItem>
        {
            new QuestionItem { Id = 101, AuthorName = "周资深", AuthorEmpId = "8001", Seniority = "veteran",
                Question = "在你职业生涯中，做出最艰难的选择是什么？", AnimalIcon = "🦁" },
            new QuestionItem { Id = 102, AuthorName = "张前辈", AuthorEmpId = "8002", Seniority = "veteran",
                Question = "在建投工作多年，最让你产生归属感的一瞬间是什么时候？", AnimalIcon = "🦊" }
        };

        private readonly List<MatchRecord> _records = new List<MatchRecord>();
        private readonly object _lock = new object();
        private int _nextId = 1000;

        // 添加问题
        public void AddQuestion(AddQuestionReq req)
        {
            lock (_lock)
            {
                var item = new QuestionItem
                {
                    Id = _nextId++,
                    AuthorName = string.IsNullOrWhiteSpace(req.AuthorName) ? "匿名" : req.AuthorName.Trim(),
                    AuthorEmpId = string.IsNullOrWhiteSpace(req.AuthorEmpId) ? "0000" : req.AuthorEmpId.Trim(),
                    Seniority = req.Seniority,
                    Question = req.Question?.Trim() ?? "（未填写问题）",
                    AnimalIcon = string.IsNullOrEmpty(req.AnimalIcon) ? "🐱" : req.AnimalIcon,
                    CreateTime = DateTime.Now
                };

                if (req.Seniority == "new")
                    _newbiePool.Add(item);
                else
                    _veteranPool.Add(item);

                // 控制台输出调试信息（可在输出窗口看到）
                Console.WriteLine($"[CardService] 添加问题：{item.Question}，所属池：{req.Seniority}");
            }
        }

        // 随机抽卡
        public QuestionItem GetRandomQuestion(string targetSeniority, string currentEmpId)
        {
            lock (_lock)
            {
                var targetPool = (targetSeniority == "new") ? _newbiePool : _veteranPool;
                var candidates = targetPool.Where(q => q.AuthorEmpId != currentEmpId).ToList();
                if (candidates.Count == 0)
                    candidates = targetPool.ToList();

                if (candidates.Count == 0)
                {
                    candidates = (targetSeniority == "new" ? _veteranPool : _newbiePool).ToList();
                }

                if (candidates.Count == 0) return null;

                var rand = new Random();
                return candidates[rand.Next(candidates.Count)];
            }
        }

        // 提交回答
        public string SubmitAnswer(SubmitAnswerReq req)
        {
            lock (_lock)
            {
                var allQ = _newbiePool.Concat(_veteranPool).ToList();
                var qItem = allQ.FirstOrDefault(q => q.Id == req.QuestionId);

                var rand = new Random();
                string code = "JTR-" + rand.Next(100000, 999999).ToString("D6");

                var record = new MatchRecord
                {
                    RecordId = _records.Count + 1,
                    QuestionId = req.QuestionId,
                    AskName = qItem?.AuthorName ?? "未知提问人",
                    AskEmpId = qItem?.AuthorEmpId ?? "无工号",
                    Question = qItem?.Question ?? "问题详情不存在",
                    AnswerName = string.IsNullOrWhiteSpace(req.AnswererName) ? "匿名" : req.AnswererName.Trim(),
                    AnswerEmpId = string.IsNullOrWhiteSpace(req.AnswererEmpId) ? "无工号" : req.AnswererEmpId.Trim(),
                    Answer = req.Answer?.Trim() ?? "（未填写回答）",
                    TicketCode = code,
                    MatchTime = DateTime.Now
                };

                _records.Add(record);
                Console.WriteLine($"[CardService] 新增匹配记录：{record.AskName} -> {record.AnswerName}，凭证：{code}");
                return code;
            }
        }

        // 获取管理后台完整数据（包含统计和记录列表）
        public object GetAdminOverview()
        {
            lock (_lock)
            {
                return new
                {
                    totalMatches = _records.Count,
                    totalNewbieQuestions = _newbiePool.Count,
                    totalVeteranQuestions = _veteranPool.Count,
                    records = _records.OrderByDescending(r => r.MatchTime).ToList(),
                    newbiePool = _newbiePool.OrderByDescending(q => q.CreateTime).ToList(),
                    veteranPool = _veteranPool.OrderByDescending(q => q.CreateTime).ToList()
                };
            }
        }
    }
}