using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Interpreter;
using Charm.Core.Domain.Services;
using Charm.Core.Domain.Utils;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Charm.Core.Domain.SpeechCases
{
    public class RemoveTaskCase : SpeechCase
    {
        private readonly CharmInterpreter _interpreter;
        private List<int>? _numbers;

        public RemoveTaskCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.AddParser("numberParser", NumberParser);

            _interpreter.SetPattern
            (
                @" удали | удалить | убери | убрать {*}>numberParser#"
            );
            var result = _interpreter.TryInterpret(message.OriginalString);

            return result;
        }

        private bool NumberParser(List<string> words)
        {
            return CharmParser.NumberParser(words, out _numbers);
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            if (_numbers is null) throw new NullReferenceException(nameof(_numbers));
            
            var gists = await manager.GetGistsFromContext(_numbers);
            manager.Context.Gists.RemoveRange(gists);
            await manager.Context.SaveChangesAsync();

            if (gists.Count > 0)
            {
                return "Удалены задачи:\n" + GistHelper.CreateGistListResponse(gists);
            }

            return "Задачи не были удалены, обновите список";
        }
    }
}