using System.Text.Json;

namespace Hypergiant.HIVE
{
    public class JsonCommandValidator : ICommandValidator
    {
        public bool IsValidCommand(string commandData)
        {
            try
            {
                JsonDocument.Parse(commandData);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
