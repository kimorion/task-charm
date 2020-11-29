using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Services;
using Charm.Core.Domain.Utils;

namespace Charm.Core.Domain.SpeechCases
{
    public class TaskListCase : SpeechCase
    {
        private ListCreationType? _listCreationType = null!;
        private DateTimeOffset? _date;

        public override bool TryParse(MessageInfo message)
        {
            // задачи
            var startSearch = message.SearchSingle("задачи");
            if (!startSearch.IsValid) return false;

            // задачи [на сегодня]
            startSearch.ToGroup().SkipNext("на").WithNextOut(CharmParser.ParseDay, out _date);

            // [[не] сделанные] задачи
            if (startSearch.ToGroup().WithPrev("сделанные").Build(out var typeSearch))
            {
                _listCreationType = typeSearch.WithPrev("не").IsValid ? ListCreationType.Undone : ListCreationType.Done;
                return true;
            }

            _listCreationType = ListCreationType.Undone;
            return true;
        }

        public async override Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            throw new System.NotImplementedException();
        }

        enum ListCreationType
        {
            Done,
            Undone
        }
    }
}