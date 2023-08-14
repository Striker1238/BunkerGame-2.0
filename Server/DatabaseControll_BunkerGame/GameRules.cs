namespace BunkerGame.GameRules
{
    public class Round
    {
        public byte RoundNumber { get; set; }
        public byte CountOpenCharacteristic { get; set; }
        public short IndexMandatoryCharacteristic { get; set; } //-1 если нет
        public byte TimeInSecondsPerStep { get; set; }
        public byte CountStep { get; set; }
        public byte StepNumber { get; set; }//Вызывать методы при изменении шага
        ///Сообщить игрокам, кто начинает ход
        public Round(byte countPlayers) : this(1, 1, 60, countPlayers) { }

        public Round(byte _RoundNumber, byte _CountOpenCharacteristic, byte _TimeInSecondsPerStep, byte CountPlayers, short MandatoryCharacteristic = -1)
        {
            RoundNumber = _RoundNumber;
            CountOpenCharacteristic = _CountOpenCharacteristic;
            TimeInSecondsPerStep = _TimeInSecondsPerStep;
            CountStep = CountPlayers;
            IndexMandatoryCharacteristic = MandatoryCharacteristic;
        }
        
    }
    public class GameRules
    {
        /// <summary>
        /// Количество раундов. Зависит от количества игроков и определяется по формуле CountPlayers/2 и округляем в большую сторону.
        /// </summary>
        public byte CountRounds;
        /// <summary>
        /// Текущий номер раунда
        /// </summary>
        public Round round { get; set; }
    }
}
