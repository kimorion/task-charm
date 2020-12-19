using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Interpreter;
using Charm.Core.Domain.Services;
using Charm.Core.Domain.Utils;

namespace Charm.Core.Domain.SpeechCases
{
    public class MarkTaskCase : SpeechCase
    {
        private readonly CharmInterpreter _interpreter;
        private List<int>? _numbers;
        private bool? _isDone;

        public MarkTaskCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.AddParser("numberParser", NumberParser);

            _interpreter.SetPattern
            (
                @"выполнено |  сделана | сделано | сделаны | готово | готовы | готова | готовая {*}>numberParser#"
            );
            var result = _interpreter.TryInterpret(message.OriginalString);

            if (result)
            {
                _isDone = true;
                return result;
            }

            _interpreter.SetPattern
            (
                @"([нужно | надо | необходимо] сделать | выполнить) |
                     (не (сделаны | выполнены | готовы | готово | готова | выполнена | сделана)) {*}>numberParser#"
            );
            result = _interpreter.TryInterpret(message.OriginalString);
            if (result)
            {
                _isDone = false;
            }

            return result;
        }

        private bool NumberParser(List<string> words)
        {
            return CharmParser.NumberParser(words, out _numbers);
        }

        public async override Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            if (_numbers is null) throw new NullReferenceException(nameof(_numbers));
            if (_isDone is null) throw new NullReferenceException(nameof(_isDone));

            var gists = await manager.GetGistsFromContext(_numbers);
            foreach (var gist in gists)
            {
                gist.IsDone = _isDone.Value;
                gist.Reminder = null;
            }

            await manager.Context.SaveChangesAsync();

            if (gists.Count > 0)
            {
                return $"Отмечены {(_isDone.Value ? "выполненными" : "не выполненными")} задачи:\n" +
                       GistHelper.CreateGistListResponse(gists);
            }

            return "Задачи не были удалены, обновите список";
        }
    }
}