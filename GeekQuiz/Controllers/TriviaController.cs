using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Description;
using GeekQuiz.Models;

namespace GeekQuiz.Controllers
{
    [Authorize]
    public class TriviaController : ApiController
    {
        private TriviaContext _db;

        public TriviaController(TriviaContext db)
        {
            _db = db;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._db.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task<TriviaQuestion> NextQuestionAsync(string userId)
        {
            var lastQuestionId = await this._db.TriviaAnswers
                .Where(a => a.UserId == userId)
                .GroupBy(a => a.QuestionId)
                .Select(g => new { QuestionId = g.Key, Count = g.Count() })
                .OrderByDescending(q => new { q.Count, QuestionId = q.QuestionId })
                .Select(q => q.QuestionId)
                .FirstOrDefaultAsync();

            var questionsCount = await this._db.TriviaQuestions.CountAsync();

            var nextQuestionId = (lastQuestionId % questionsCount) + 1;
            return await this._db.TriviaQuestions.FindAsync(CancellationToken.None, nextQuestionId);
        }

        [ResponseType(typeof(TriviaQuestion))]
        public async Task<IHttpActionResult> Get()
        {
            var userId = User.Identity.Name;

            TriviaQuestion nextQuestion = await this.NextQuestionAsync(userId);

            if (nextQuestion == null)
            {
                return this.NotFound();
            }

            return this.Ok(nextQuestion);
        }

        private async Task<bool> StoreAsync(TriviaAnswer answer)
        {
            this._db.TriviaAnswers.Add(answer);

            await this._db.SaveChangesAsync();
            var selectedOption = await this._db.TriviaOptions.FirstOrDefaultAsync(o => o.Id == answer.OptionId
                                                                                      && o.QuestionId == answer.QuestionId);

            return selectedOption.IsCorrect;
        }

        // POST api/Trivia
        [ResponseType(typeof(TriviaAnswer))]
        public async Task<IHttpActionResult> Post(TriviaAnswer answer)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            answer.UserId = User.Identity.Name;

            var isCorrect = await this.StoreAsync(answer);
            return this.Ok<bool>(isCorrect);
        }
    }
}
