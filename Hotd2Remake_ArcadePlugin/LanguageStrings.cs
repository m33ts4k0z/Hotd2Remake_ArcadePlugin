using System;
using System.Collections.Generic;

namespace Hotd2Remake_ArcadePlugin
{
    public class LanguageStrings
    {
        public enum StringName
        {
            Credits,
            Continue,
            EnterYourName,
            Freeplay,
            GameOver,
            InsertCoins,
            InsertMoreCoins,
            Loading,
            PressStart,
            PressStartToSkip,
            Ranking,
            Score
        }

        public static Dictionary<StringName, String> ChineseDictionnary = new Dictionary<StringName, String>()
        {
            //Traditional Chinese, like the game (zh-HK)
            { StringName.Credits, "學分"},
            { StringName.Continue, "繼續 ？"},
            { StringName.EnterYourName, "輸入你的名字"},
            { StringName.Freeplay, "免費玩"},
            { StringName.GameOver, "遊戲結束"},
            { StringName.InsertCoins, "投幣"},
            { StringName.InsertMoreCoins, "插入更多硬幣"},
            { StringName.Loading, "載入中。 . ."},
            { StringName.PressStart, "按開始"},
            { StringName.PressStartToSkip, "按開始跳過"},
            { StringName.Ranking, "排行榜"},
            { StringName.Score, "得分"}
        };

        public static Dictionary<StringName, String> EnglishDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CREDITS"},
            { StringName.Continue, "CONTINUE ?"},
            { StringName.EnterYourName, "ENTER YOUR NAME"},
            { StringName.Freeplay, "FREEPLAY"},
            { StringName.GameOver, "GAME OVER"},
            { StringName.InsertCoins, "INSERT COIN(S)"},
            { StringName.InsertMoreCoins, "INSERT MORE COIN(S)"},
            { StringName.Loading, "LOADING. . ."},
            { StringName.PressStart, "PRESS START"},
            { StringName.PressStartToSkip, "PRESS START TO SKIP"},
            { StringName.Ranking, "RANKING"},
            { StringName.Score, "SCORE"}
        };

        public static Dictionary<StringName, String> FrenchDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CRÉDITS"},
            { StringName.Continue, "CONTINUER ?"},
            { StringName.EnterYourName, "ENTREZ VOTRE NOM"},
            { StringName.Freeplay, "JEU LIBRE"},
            { StringName.GameOver, "GAME OVER"},      
            { StringName.InsertCoins, "INSÉRER CRÉDIT(S)"},
            { StringName.InsertMoreCoins, "INSÉRER PLUS DE CRÉDIT(S)"},
            { StringName.Loading, "CHARGEMENT EN COURS. . ."},
            { StringName.PressStart, "APPUYER SUR START"},
            { StringName.PressStartToSkip, "APPUYER SUR START POUR PASSER"},
            { StringName.Ranking, "CLASSEMENT"},
            { StringName.Score, "SCORE"}
        };

        public static Dictionary<StringName, String> GermanDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "KREDITE"},
            { StringName.Continue, "FORTSETZEN ?"},
            { StringName.EnterYourName, "NAMEN EINGEBEN"},
            { StringName.Freeplay, "FREIES SPIEL"},
            { StringName.GameOver, "SPIEL IST AUS"},      
            { StringName.InsertCoins, "MÜNZEN EINWERFEN)"},
            { StringName.InsertMoreCoins, "WEITERE MÜNZE(N) EINWERFEN"},
            { StringName.Loading, "WIRD GELADEN. . ."},
            { StringName.PressStart, "DRÜCKE START"},
            { StringName.PressStartToSkip, "DRÜCKE START ZUM ÜBERSPRINGEN"},
            { StringName.Ranking, "RANGLISTE"},
            { StringName.Score, "PUNKTE"}
        };

        public static Dictionary<StringName, String> ItalianDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CREDITI"},
            { StringName.Continue, "CONTINUA ?"},
            { StringName.EnterYourName, "INSERISCI IL TUO NOME"},
            { StringName.Freeplay, "GIOCO GRATUITO"},
            { StringName.GameOver, "GAME OVER"},      
            { StringName.InsertCoins, "INSERIRE MONETA(E)"},
            { StringName.InsertMoreCoins, "INSERIRE ALTRE MONETE"},
            { StringName.Loading, "CARICAMENTO IN CORSO. . ."},
            { StringName.PressStart, "PREMI START"},
            { StringName.PressStartToSkip, "PREMI START PER SALTARE"},
            { StringName.Ranking, "CLASSIFICA"},
            { StringName.Score, "PUNTEGGIO"}
        };

        public static Dictionary<StringName, String> JapaneseDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "クレジット"},
            { StringName.Continue, "続く ？"},
            { StringName.EnterYourName, "名前を入力してください"},
            { StringName.Freeplay, "無料プレイ"},
            { StringName.GameOver, "ゲームオーバー"},      
            { StringName.InsertCoins, "コインを挿入"},
            { StringName.InsertMoreCoins, "より多くのコインを挿入してください"},
            { StringName.Loading, "読み込み中。 . ."},
            { StringName.PressStart, "スタートを押す"},
            { StringName.PressStartToSkip, "スキップするにはスタートを押してください"},
            { StringName.Ranking, "ランキング"},
            { StringName.Score, "スコア"}
        };

        public static Dictionary<StringName, String> PolishDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "KREDYTY"},
            { StringName.Continue, "DALEJ ?"},
            { StringName.EnterYourName, "WPISZ SWOJE IMIĘ"},
            { StringName.Freeplay, "DARMOWA GRA"},
            { StringName.GameOver, "KONIEC GRY"},      
            { StringName.InsertCoins, "WŁÓŻ MONETY"},
            { StringName.InsertMoreCoins, "WŁÓŻ WIĘCEJ MONETY"},
            { StringName.Loading, "ŁADOWANIE. . ."},
            { StringName.PressStart, "NACIŚNIJ START"},
            { StringName.PressStartToSkip, "NACIŚNIJ START, ABY POMINĄĆ"},
            { StringName.Ranking, "RANKING"},
            { StringName.Score, "WYNIK"}
        };

        public static Dictionary<StringName, String> PortugueseDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CRÉDITOS"},
            { StringName.Continue, "CONTINUAR ?"},
            { StringName.EnterYourName, "INTRODUZA O SEU NOME"},
            { StringName.Freeplay, "JOGO LIVRE"},
            { StringName.GameOver, "FIM DE JOGO"},
            { StringName.InsertCoins, "INSIRA MOEDA(S)"},
            { StringName.InsertMoreCoins, "INSIRA MAIS MOEDA(S)"},
            { StringName.Loading, "A CARREGAR. . ."},
            { StringName.PressStart, "PRIMA START"},
            { StringName.PressStartToSkip, "PRIMA START PARA SALTAR"},
            { StringName.Ranking, "CLASSIFICAÇÃO"},
            { StringName.Score, "PONTUAÇÃO"}
        };

        public static Dictionary<StringName, String> SpainDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CRÉDITOS"},
            { StringName.Continue, "SEGUIR ?"},
            { StringName.EnterYourName, "INTRODUCE TU NOMBRE"},
            { StringName.Freeplay, "JUEGO GRATIS"},
            { StringName.GameOver, "JUEGO TERMINADO"},      
            { StringName.InsertCoins, "INSERTAR MONEDA(S)"},
            { StringName.InsertMoreCoins, "INSERTAR MÁS MONEDA(S)"},
            { StringName.Loading, "CARGANDO. . ."},
            { StringName.PressStart, "PRESIONA INICIO"},
            { StringName.PressStartToSkip, "PULSA INICIO PARA SALTAR"},
            { StringName.Ranking, "CLASIFICACIÓN"},
            { StringName.Score, "PUNTUACIÓN"}
        };
    }
}









