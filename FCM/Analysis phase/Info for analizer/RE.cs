namespace FCM
{
    public static class RE
    {
        public const string Invoc = NormalInvoc + "|" + ThreadInvoc;
        public const string NormalInvoc = @"^(?!.*?(new[\S\s]+?)).*\(.*\);";
        public const string ThreadInvoc = @".*new[\S\s]+?Thread[\S\s]*?\(.*\)\.Start\(\);";
        public const string IfElse = If + Else;
        public const string If = @"if[\S\s]*?\(*\)[\S\s]*?(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)[\S\s]*?";
        public const string Else = @"else(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)";
        public const string ForOrForEachOrWhileOrDoWhile = ForOrForEachOrWhile + "|" + DoWhile;
        public const string DoWhile = @"(for|while)(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)|do(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)while";
        public const string ForOrForEachOrWhile = @"(for|foreach|while)(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)";
        public const string TryCatch = Try + Catch;
        public const string Try = @"try(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)[\S\s]*?";
        public const string Catch = @"catch[\S\s]*?\(.*\)[\S\s]*?(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)";
        public const string Switch = @"switch[\S\s]*?\(.*\)[\S\s]*?(.*[^\{[^}]*\}|//.*\r?\n|[\S\s]*?\}]*)";
        public const string IfElseOrSwitch_OR_ForOrWhile_OR_TryCatch = IfElse + "|" + ForOrForEachOrWhileOrDoWhile + "|" + TryCatch + "|" + Switch;
        public const string Function = @"(?<signature>[^{]*)(?<body>(?:\{[^}]*\}|//.*\r?\n|[\S\s])\(*\)[\S\s]*?\{(?:\{[^}]*\}|//.*\r?\n|[\S\s])*?)\}";
    }
}
