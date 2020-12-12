using System.Text;
using System.Threading.Tasks;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Interpreter;
using Charm.Core.Domain.Services;

namespace Charm.Core.Domain.SpeechCases
{
    public class HelpCase : SpeechCase
    {
        private readonly CharmInterpreter _interpreter;

        public HelpCase(CharmInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public override bool TryParse(MessageInfo message)
        {
            _interpreter.SetPattern
            (
                @"помощь | help | справка | команды | инструкция#"
            );
            var result = _interpreter.TryInterpret(message.OriginalString);
            return result;
        }

        public override async Task<string> ApplyAndRespond(long userId, CharmManager manager)
        {
            var helpBuilder = new StringBuilder();
            helpBuilder.AppendLine("<b>Доступные команды:</b>");
            
            helpBuilder.AppendLine("<b>Создание задачи:</b>");
            helpBuilder.Append("<i>");
            helpBuilder.AppendLine("в понедельник в 17 часов занятие английского");
            helpBuilder.AppendLine("через час встреча с начальником");
            helpBuilder.AppendLine("заменить трубу в ванной");
            helpBuilder.AppendLine("</i>");
            
            helpBuilder.AppendLine("<b>Составление списка задач:</b>");
            helpBuilder.Append("<i>");
            helpBuilder.AppendLine("все задачи на сегодня");
            helpBuilder.AppendLine("список дел на завтра");
            helpBuilder.AppendLine("задачи");
            helpBuilder.AppendLine("</i>");
            
            helpBuilder.AppendLine("<b>Установка статуса выполнения: (передайте номер задачи из последнего списка)</b>");
            helpBuilder.Append("<i>");
            helpBuilder.AppendLine("готова 5");
            helpBuilder.AppendLine("не готовы 1 2 3");
            helpBuilder.AppendLine("</i>");
            
            helpBuilder.AppendLine("<b>Удаление задачи: (передайте номер задачи из последнего списка)</b>");
            helpBuilder.Append("<i>");
            helpBuilder.AppendLine("удали 5");
            helpBuilder.AppendLine("убрать 1 2 3");
            helpBuilder.AppendLine("</i>");
            
            helpBuilder.AppendLine("<b>Добавление напоминания: (передайте номер задачи из последнего списка)</b>");
            helpBuilder.Append("<i>");
            helpBuilder.AppendLine("создай напоминание о 5 на завтра в 20:00");
            helpBuilder.AppendLine("напомни про 5 завтра");
            helpBuilder.AppendLine("</i>");

            return helpBuilder.ToString();
        }
    }
}