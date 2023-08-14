using BunkerGame.Server;
public class Hero
    {
        /// <summary>
        /// Index - 0
        /// </summary>
        public int? Age_Hero { get; set; }
        /// <summary>
        /// Index - 1
        /// </summary>
        public string? Profession_Hero { get; set; }
        /// <summary>
        /// Index - 2
        /// </summary>
        private int? experienceProfession_Hero { get; set; }
        public int? ExperienceProfession_Hero
        {
            get => experienceProfession_Hero;
            set { experienceProfession_Hero = ((Age_Hero - 16) >= MathF.Ceiling((float)value / 12f)) ? value : (byte)Math.Min((int)(Age_Hero - 16), 1); }
        }

        /// <summary>
        /// Index - 3
        /// </summary>
        public bool? Sex_Hero { get; set; }

        /// <summary>
        /// Index - 4
        /// </summary>
        public string? Hobbies_Hero { get; set; }
        /// <summary>
        /// Index - 5
        /// </summary>
        private int? experienceHobbies_Hero { get; set; }
        public int? ExperienceHobbies_Hero
        {
            get => experienceHobbies_Hero;
            set { experienceHobbies_Hero = ((Age_Hero - 16) >= MathF.Ceiling((float)value / 12f)) ? value : (byte)Math.Min((int)(Age_Hero - 16), 1); }
        }

        /// <summary>
        /// Index - 6
        /// </summary>
        public string? Luggage_Hero { get; set; }
        /// <summary>
        /// Index - 7
        /// </summary>
        public string? HealthCondition_Hero { get; set; }
        /// <summary>
        /// Index - 8
        /// </summary>
        private int? healthPoint_Hero { get; set; }
        public int? HealthPoint_Hero
        {
            get => healthPoint_Hero;
            set
            {

                healthPoint_Hero = (!ServerObject.HealthCondition.Find(x => x.translation[1].Profession == HealthCondition_Hero).Whether_Measured)
                    ? 0
                    : value;
                HealthCondition_Hero = (healthPoint_Hero == 0) ? "Нет болезней" : HealthCondition_Hero;
            }
        }
        /// <summary>
        /// Index - 9
        /// </summary>
        public string? Phobia_Hero { get; set; }
        /// <summary>
        /// Index - 10
        /// </summary>
        private int? phobiaPercentage_Hero { get; set; }
        public int? PhobiaPercentage_Hero
        {
            get => phobiaPercentage_Hero;
            set
            {

                phobiaPercentage_Hero = (!ServerObject.Phobia.Find(x => x.translation[1].Profession == Phobia_Hero).Whether_Measured)
                    ? 0
                    : value;
                Phobia_Hero = (phobiaPercentage_Hero == 0) ? "Нет фобии" : Phobia_Hero;
            }
        }
        /// <summary>
        /// Index - 11
        /// </summary>
        public string? HumanTrait_Hero { get; set; }
        /// <summary>
        /// Index - 12
        /// </summary>
        public string? FurtherInformation_Hero { get; set; }
        /// <summary>
        /// Index - 13
        /// </summary>
        public string? BodyType_Hero { get; set; }
        /// <summary>
        /// Index - 14
        /// </summary>
        public int? BodyPrecentage_Hero { get; set; }

    }

