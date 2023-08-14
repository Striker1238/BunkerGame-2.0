using BunkerGame.Server;
using MongoDB.Bson.Serialization.Attributes;

[System.Serializable]
[BsonIgnoreExtraElements]
public class Player
{
    public Hero hero { get; set; }
    public User user { get; set; }
    public ClientObject client;
    public bool isReady { get; set; }

    public bool showCharacteristics;
    protected internal async Task<string[]> ReturnCharacteristics(byte[] indexCharacteristics)
    {
        //характеристика в формате Json
        string[] characteristics = new string[indexCharacteristics.Length];
        for (int i = 0; i < indexCharacteristics.Length; i++)
        {
            switch (indexCharacteristics[i])
            {
                case 0:
                    characteristics[i] = hero.Age_Hero.ToString();
                    break;
                case 1:
                    characteristics[i] = hero.Profession_Hero;
                    break;
                case 2:
                    characteristics[i] = hero.ExperienceProfession_Hero.ToString();
                    break;
                case 3:
                    characteristics[i] = hero.Sex_Hero.ToString();
                    break;
                case 4:
                    characteristics[i] = hero.Hobbies_Hero;
                    break;
                case 5:
                    characteristics[i] = hero.ExperienceHobbies_Hero.ToString();
                    break;
                case 6:
                    characteristics[i] = hero.Luggage_Hero;
                    break;
                case 7:
                    characteristics[i] = hero.HealthCondition_Hero;
                    break;
                case 8:
                    characteristics[i] = hero.HealthPoint_Hero.ToString();
                    break;
                case 9:
                    characteristics[i] = hero.Phobia_Hero;
                    break;
                case 10:
                    characteristics[i] = hero.PhobiaPercentage_Hero.ToString();
                    break;
                case 11:
                    characteristics[i] = hero.HumanTrait_Hero;
                    break;
                case 12:
                    characteristics[i] = hero.FurtherInformation_Hero;
                    break;
                case 13:
                    characteristics[i] = hero.BodyType_Hero;
                    break;
                case 14:
                    characteristics[i] = hero.BodyPrecentage_Hero.ToString();
                    break;
            }
        }
        return characteristics;
    }
}