using System.Text;

namespace pokemon_discord_bot
{
    public enum TextColor
    {
        Gray = 30,
        Red = 31,
        Green = 32,
        Yellow = 33,
        Blue = 34,
        Pink = 35,
        Cyan = 36,
        White = 37,

        Default = 39
    }

    public enum BackgroundColor
    {
        DarkBlue = 40,
        Orange = 41,
        MarbleBlue = 42,
        GrayTurquoise = 43,
        Gray = 44,
        Indigo = 45,
        LightGray = 46,
        White = 47,

        Default = 49
    }

    public class AnsiBuilder
    {
        private StringBuilder _builder = new();
        private bool _hasContent = false;

        public AnsiBuilder()
        {
            _builder.AppendLine("```ansi");
        }

        public AnsiBuilder WithLine(string text, TextColor textColor = TextColor.Default, BackgroundColor backgroundColor = BackgroundColor.Default, bool bold = false, bool underline = false) 
        {
            return WithFormat(text, textColor, backgroundColor, bold, underline, true);
        }

        public AnsiBuilder WithText(string text, TextColor textColor = TextColor.Default, BackgroundColor backgroundColor = BackgroundColor.Default, bool bold = false, bool underline = false) 
        {
            return WithFormat(text, textColor, backgroundColor, bold, underline, false);
        }

        public AnsiBuilder WithBlankSpace()
        {
            _builder.Append("\n");
            return this;
        }

        private AnsiBuilder WithFormat(string text, TextColor textColor, BackgroundColor backgroundColor, bool bold, bool underline, bool newLine)
        {
            if (string.IsNullOrEmpty(text)) return this;

            if (newLine && _hasContent) _builder.Append('\n');
            
            _builder.Append("\u001b[");

            // Format
            var codes = new List<string>();
            if (bold) codes.Add("1");
            if (underline) codes.Add("4");
            _builder.Append($"{string.Join(";", codes)}");

            // Add separator if needed
            if (codes.Count > 0) _builder.Append(";");

            // Color
            _builder.Append($"{(int)textColor};{(int)backgroundColor}m");

            // Text and Reset
            _builder.Append(text);
            _builder.Append("\u001b[0m");
            _hasContent = true;
            return this;
        }

        public string Build()
        {
            //Only append newline if there isn't one already
            if (_builder.Length > 0 && _builder[^1] != '\n')
                _builder.Append('\n');

            _builder.Append("```");
            return _builder.ToString();
        }
    }
}
