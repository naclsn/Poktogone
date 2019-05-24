using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poktogone.Main;
using Poktogone.Pokemon;

namespace Poktogone.Battle
{
    class Calc
    {
        /// <summary>
        /// Calculate and apply the damage of a move from an attacking pokémon, to the defending pokémon of the defending trainer,
        /// while accounding for the stage's weather, terrain [and other...].
        /// </summary>
        /// <param name="stage">Context for the actions.</param>
        /// <param name="atk">Attacking pokémon.</param>
        /// <param name="def">Defending pokémon.</param>
        /// <param name="atkTrainer">Trainer of the attacking pokémon.</param>
        /// <param name="defTrainer">Trainer of the defending pokémon.</param>
        /// <returns>The damage inflicted, in percents.</returns>
        public static int DamageCalculator(Stage stage, Set atk, Set def, Trainer atkTrainer, Trainer defTrainer)
        {
            int damageInflicted = 0;
            if (atk.HasFlags(Flags.Flinch) && atk.ability.id != 9) //Flinch
            {
                return 0;
            }
            if (atk.Status == Status.Sleep)//Sleep
                return 0;

            if (atk.Status == Status.Paralysis && Program.RngNext(3) == 0)//Paralysis
                return 0;


            if (def.ability.id == 2 && atk.NextMove.type == Pokemon.Type.Electrik)//Lightningrod
            {
                def[StatTarget.AttackSpecial] = 1;
                return 0;
            }
            if (def.ability.id == 16 && atk.NextMove.type == Pokemon.Type.Feu)//Flashfire
            {
                return 0;
            }
            if (def.ability.id == 19 && atk.NextMove.type == Pokemon.Type.Plante)//Sapsipper
            {
                def[StatTarget.AttackSpecial] = 1;
                return 0;
            }
            if (def.ability.id == 36 && atk.NextMove.type == Pokemon.Type.Sol)//Levitate
            {
                if (!(def.HasFlags(Flags.Roost)) && atk.ability.id != 51)//ExceptionRoost&MoldBreaker
                    return 0;
            }
            if (def.ability.id == 54 && atk.NextMove.id == 77)//Bulletproof
            {
                return 0;
            }
            if (atk.HasFlags(Flags.Recharge))
            {
                atk.RemoveFlags(Flags.Recharge);
                Program.Print("Le pokémon doit se recharger !");
                return 0;
            }
            if (atk.item.id == 4 || atk.item.id == 5 || atk.item.id == 6 || atk.NextMove[34] != null)
            {
                atk.AddFlags(Flags.Locked);
            }


            //====================SUPPORT=====================//

            if (atk.NextMove.sps == Sps.Stat) //Support
            {
                if (atk.NextMove[1] != null)//Climat
                {
                    stage.Weather = (WeatherType)atk.NextMove[1].Value.value;
                }
                if (atk.NextMove[2] != null)//Brulure
                {
                    if (def.ability.id == 76)
                    {
                        atk.Status = Status.Burn;
                    }
                    else
                    {
                        def.Status = Status.Burn;
                    }
                }
                if (atk.NextMove[7] != null)//AtkBoost
                {
                    atk[StatTarget.Attack] = atk.NextMove[7].Value.value;
                }
                if (atk.NextMove[8] != null)//DefBoost
                {
                    atk[StatTarget.Defence] = atk.NextMove[8].Value.value;
                }
                if (atk.NextMove[9] != null)//SpaBoost
                {
                    atk[StatTarget.AttackSpecial] = atk.NextMove[9].Value.value;
                }
                if (atk.NextMove[10] != null)//SpdBoost
                {
                    atk[StatTarget.DefenceSpecial] = atk.NextMove[10].Value.value;
                }
                if (atk.NextMove[11] != null)//SpeBoost
                {
                    atk[StatTarget.Speed] = atk.NextMove[11].Value.value;
                }
                if (atk.NextMove[13] != null)//Soins
                {
                    atk.Hp += atk.Hp * atk.NextMove[13].Value.value / 100;
                }
                if (atk.NextMove[14] != null)//Sleep
                {
                    if (def.ability.id == 76)
                    {
                        atk.Status = Status.Sleep;
                    }
                    else
                    {
                        def.Status = Status.Sleep;
                    }
                }
                if (atk.NextMove[17] != null)//InflictDamage
                {
                    def.Hp -= def.GetMaxHp() * atk.NextMove[17].Value.percent / 100 + atk.NextMove[17].Value.value;
                }
                if (atk.NextMove.id == 22)//if (atk.NextMove[18] != null)//Leechseed
                {
                    if (def.ability.id == 76)
                    {
                        atk.AddFlags(Flags.LeechSeed);
                    }
                    else
                    {
                        def.AddFlags(Flags.LeechSeed);
                    }
                }
                if (atk.NextMove[20] != null)//Protection
                {
                    atk.AddFlags(Flags.Protect);
                }
                if (atk.NextMove[21] != null)//Paralysis
                {
                    if (def.ability.id == 76)
                    {
                        atk.Status = Status.Paralysis;
                    }
                    else
                    {
                        def.Status = Status.Paralysis;
                    }
                }
                if (atk.NextMove[24] != null)//Substitute
                {
                    atk.Hp -= (int)(atk.GetMaxHp() * 0.25);
                    atk.AddFlags(Flags.Substitute);
                }
                if (atk.NextMove[26] != null)//RemoveHazards
                {
                    bool hasReflect = false;
                    bool hasLightScreen = true;

                    if (atkTrainer.HasHazards(Hazards.Reflect)) { hasReflect = true; }
                    if (atkTrainer.HasHazards(Hazards.LightScreen)) { hasLightScreen = true; }

                    defTrainer.RemoveHazards();
                    atkTrainer.RemoveHazards();

                    if (hasReflect) { atkTrainer.AddHazards(Hazards.Reflect); }
                    if (hasLightScreen) { atkTrainer.AddHazards(Hazards.LightScreen); }
                }
                if (atk.NextMove[38] != null)//StealthRock
                {
                    if (def.ability.id == 76)
                    {
                        atkTrainer.AddHazards(Hazards.StealthRock);
                    }
                    else
                    {
                        defTrainer.AddHazards(Hazards.StealthRock);
                    }
                }
                if (atk.NextMove[39] != null)//Spikes
                {
                    if (def.ability.id == 76)
                    {
                        if (atkTrainer.HasHazards(Hazards.Spikes3)) { }

                        else if (atkTrainer.HasHazards(Hazards.Spikes2))
                        {
                            atkTrainer.RemoveHazards(Hazards.Spikes2);
                            atkTrainer.AddHazards(Hazards.Spikes3);
                        }

                        else if (atkTrainer.HasHazards(Hazards.Spikes))
                        {
                            atkTrainer.RemoveHazards(Hazards.Spikes);
                            atkTrainer.AddHazards(Hazards.Spikes2);
                        }

                        else
                        {
                            atkTrainer.AddHazards(Hazards.Spikes);
                        }
                    }
                    else
                    {
                        if (defTrainer.HasHazards(Hazards.Spikes3)) { }

                        else if (defTrainer.HasHazards(Hazards.Spikes2))
                        {
                            defTrainer.RemoveHazards(Hazards.Spikes2);
                            defTrainer.AddHazards(Hazards.Spikes3);
                        }

                        else if (defTrainer.HasHazards(Hazards.Spikes))
                        {
                            defTrainer.RemoveHazards(Hazards.Spikes);
                            defTrainer.AddHazards(Hazards.Spikes2);
                        }

                        else
                        {
                            defTrainer.AddHazards(Hazards.Spikes);
                        }
                    }


                }
                if (atk.NextMove[41] != null)//Poison
                {
                    if (def.ability.id == 76)
                    {
                        atk.Status = Status.Poison;
                    }
                    else
                    {
                        def.Status = Status.Poison;
                    }
                }
                if (atk.NextMove[42] != null)//Toxic
                {
                    if (def.ability.id == 76)
                    {
                        atk.Status = Status.BadlyPoisoned;
                    }
                    else
                    {
                        def.Status = Status.BadlyPoisoned;
                    }
                }
                if (atk.NextMove[44] != null)//ToxicSpikes
                {
                    if (def.ability.id == 76)
                    {
                        if (atkTrainer.HasHazards(Hazards.ToxicSpikes))
                        {
                            atkTrainer.RemoveHazards(Hazards.ToxicSpikes);
                            atkTrainer.AddHazards(Hazards.ToxicSpikes2);
                        }

                        else
                        {
                            atkTrainer.AddHazards(Hazards.ToxicSpikes);
                        }
                    }
                    else
                    {
                        if (defTrainer.HasHazards(Hazards.ToxicSpikes))
                        {
                            defTrainer.RemoveHazards(Hazards.ToxicSpikes);
                            defTrainer.AddHazards(Hazards.ToxicSpikes2);
                        }

                        else
                        {
                            defTrainer.AddHazards(Hazards.ToxicSpikes);
                        }
                    }
                }
                if (atk.NextMove[45] != null)//Taunt
                {
                    if (def.ability.id == 76)
                    {
                        atk.AddFlags(Flags.Taunt);
                    }
                    else
                    {
                        atk.AddFlags(Flags.Taunt);
                    }
                }
                if (atk.NextMove[46] != null)//Trick
                {
                    Item itemTmp = atk.item;
                    atk.item = def.item;
                    def.item = itemTmp;
                }
                if (atk.NextMove[47] != null)//Reflect
                {
                    atkTrainer.AddHazards(Hazards.Reflect);
                }
                if (atk.NextMove[48] != null)//lightScreen
                {
                    atkTrainer.AddHazards(Hazards.LightScreen);
                }
                if (atk.NextMove[49] != null)//HealingWish
                {
                    atk.Hp = 0;
                    atkTrainer.AddHazards(Hazards.HealingWish);
                }
                if (atk.NextMove[50] != null)//StickyWeb
                {
                    if (def.ability.id == 76)
                    {
                        atkTrainer.AddHazards(Hazards.StickyWeb);
                    }
                    else
                    {
                        defTrainer.AddHazards(Hazards.StickyWeb);
                    }
                }
                if (atk.NextMove[51] != null)//Roost
                {
                    atk.AddFlags(Flags.Roost);
                }
                if (atk.NextMove[52] != null)//Haze
                {
                    atk[StatTarget.Attack] = 0;
                    atk[StatTarget.Defence] = 0;
                    atk[StatTarget.AttackSpecial] = 0;
                    atk[StatTarget.DefenceSpecial] = 0;
                    atk[StatTarget.Speed] = 0;
                    def[StatTarget.Attack] = 0;
                    def[StatTarget.Defence] = 0;
                    def[StatTarget.AttackSpecial] = 0;
                    def[StatTarget.DefenceSpecial] = 0;
                    def[StatTarget.Speed] = 0;
                }
                return damageInflicted;
            }

            //====================PHISICAL=====================//

            if (atk.NextMove.sps == Sps.Physic)
            {
                if (atk.NextMove.id == 36 && atk.GetNbTurns() > 1)//Bluff
                {
                    Program.Print("Mais ça n'a aucun effet.");
                }

                if (atk.ability.id == 56)//Protean
                {
                    atk.Type1 = atk.NextMove.type;
                    atk.Type2 = Pokemon.Type.None;
                }

                int attackStat = atk[StatTarget.Attack];
                int attackPower = atk.NextMove.power;
                int defenseStat = def[StatTarget.Defence];

                double atkItemMod = 1;
                if (atk.item.id == 5) { atkItemMod *= 1.5; }//ChoiceBand
                else if (atk.item.id == 10)//LifeOrb
                {
                    atkItemMod *= 1.3;
                    if (atk.ability.id != 10 && atk.ability.id != 17)
                        atk.Hp -= (int)(atk.GetMaxHp() / 10);
                }

                double stabMod = 1;
                if (atk.IsStab(atk.NextMove.type)) { stabMod *= 1.5; }//STAB

                double abilityMod = 1;
                if (atk.ability.id == 17) { abilityMod *= 1.3; }//SheerForce
                else if (atk.ability.id == 20) { abilityMod *= 2; }//HugePower
                else if (atk.ability.id == 22 && atk.NextMove.power < 60) { abilityMod *= 1.5; }//Technician
                else if (atk.ability.id == 3 && atk.Hp < (int) (atk.GetMaxHp()/3f) && atk.NextMove.type == Pokemon.Type.Feu) { abilityMod *= 1.3; }//Blaze
                else if (atk.ability.id == 4 && atk.Hp < (int)(atk.GetMaxHp() / 3f) && atk.NextMove.type == Pokemon.Type.Plante) { abilityMod *= 1.3; }//Overgrow
                else if (atk.ability.id == 5 && atk.Hp < (int)(atk.GetMaxHp() / 3f) && atk.NextMove.type == Pokemon.Type.Eau) { abilityMod *= 1.3; }//Torrent
                else if (atk.ability.id == 21 && atk.Hp < (int)(atk.GetMaxHp() / 3f) && atk.NextMove.type == Pokemon.Type.Insecte) { abilityMod *= 1.3; }//Swarm
                else if (atk.ability.id == 73) { abilityMod *= 1.3; }

                    /*Meteo*/
                    if (stage.Weather == WeatherType.Rain)//RainModifiers
                {
                    if (atk.NextMove.type == Pokemon.Type.Eau)
                    {
                        stabMod *= 1.5;
                    }
                    else if (atk.NextMove.type == Pokemon.Type.Feu)
                    {
                        stabMod *= 0.5;
                    }
                }
                if (stage.Weather == WeatherType.HarshSunlight)//Sunmodifiers
                {
                    if (atk.NextMove.type == Pokemon.Type.Eau)
                    {
                        stabMod *= 0.5;
                    }
                    else if (atk.NextMove.type == Pokemon.Type.Feu)
                    {
                        stabMod *= 1.5;
                    }
                }

                /*Terrain*/
                if (!(atk.IsStab(Pokemon.Type.Vol) || atk.ability.id == 36))
                {
                    if (stage.Terrain == TerrainType.Eletric)
                    {
                        if (atk.NextMove.type == Pokemon.Type.Electrik)
                        {
                            stabMod *= 1.5;
                        }
                        if (atk.NextMove[14] != null)
                        {
                            return 0;
                        }
                    }
                    if (stage.Terrain == TerrainType.Psychic)
                    {
                        if (atk.NextMove.type == Pokemon.Type.Psy)
                        {
                            stabMod *= 1.5;
                        }
                        if (atk.NextMove[4] != null)
                        {
                            return 0;
                        }
                    }
                }

                if (defTrainer.HasHazards(Hazards.Reflect))//Reflect
                    stabMod *= 0.5;

                double typeMod = Program.GetMatchup(atk.NextMove.type, def.Type1, def.Type2);
                
                if (atk.ability.id == 59 && atk.NextMove.type == Pokemon.Type.Normal)//Pixilate
                {
                    typeMod = Program.GetMatchup(Pokemon.Type.Fée, def.Type1, def.Type2);
                    typeMod *= 1.2;
                }
                if (atk.ability.id == 65 && atk.NextMove.type == Pokemon.Type.Normal)//LiquidVoice
                {
                    typeMod = Program.GetMatchup(Pokemon.Type.Eau, def.Type1, def.Type2);
                    typeMod *= 1.2;
                }

                if (def.HasFlags(Flags.Roost))//RoostEffectOnFlyingTypes
                {
                    if (def.Type1 == Pokemon.Type.Vol)
                    {
                        typeMod = Program.GetMatchup(atk.NextMove.type, def.Type2);
                    }
                    else if (def.Type2 == Pokemon.Type.Vol)
                    {
                        typeMod = Program.GetMatchup(atk.NextMove.type, def.Type1);
                    }
                }

                if (def.ability.id == 47 && (atk.NextMove.type == Pokemon.Type.Feu) || (atk.NextMove.type == Pokemon.Type.Glace))//ThickFat
                    typeMod *= 0.5;

                if (def.ability.id == 12 && atk.GetMod(StatTarget.Attack) > 0)//Unaware
                {
                    stabMod = (int)(stabMod / (1 + .5 * atk.GetMod(StatTarget.Attack)));
                }

                if (typeMod == 0)//Immunité
                {
                    if (atk.NextMove[27] != null)//HighJumpKick
                    {
                        atk.Hp -= atk.GetMaxHp() / 2;
                    }
                    return 0;
                }

                if (atk.Status == Status.Burn) { typeMod *= 1 / 2; }//Burn

                if (atk.NextMove[37] != null && stage.Weather == WeatherType.Rain)//Cas particulier de Fatal-foudre et Blizzard
                {
                    damageInflicted = (int)((((42 * attackStat * attackPower / defenseStat) / 50) + 2) * stabMod * typeMod * abilityMod * atkItemMod);
                }
                else if (Program.RngNext(1, 100) <= atk.NextMove.accuracy)//Hit
                {
                    damageInflicted = (int)((((42 * attackStat * attackPower / defenseStat) / 50) + 2) * stabMod * typeMod * abilityMod * atkItemMod);
                }
                else//Miss
                {
                    if (atk.NextMove[27] != null)//HighJumpKick
                    {
                        atk.Hp -= atk.GetMaxHp() / 2;
                    }
                    Program.Print(string.Format("{0} Rate son attaque", atk.GetName()));
                    return 0;
                }

                if (atk.ability.id == 17)//SheerForce
                {
                    return damageInflicted;
                }

                EffectGen(stage, atk, def, atkTrainer, defTrainer, ref damageInflicted); //Effects

                if (def.item.id == 11)//RockyHelmet
                {
                    atk.Hp -= (int)(atk.GetMaxHp() / 8);
                }


            }

            //====================SPECIAL=====================//

            if (atk.NextMove.sps == Sps.Special) //Special
            {
                if (atk.NextMove.id == 123)
                {
                    if (atk.HasFlags(Flags.Charge) || stage.Weather == WeatherType.HarshSunlight)
                    {
                        atk.RemoveFlags(Flags.Charge);
                    }
                    else
                    {
                        atk.AddFlags(Flags.Charge);
                    }
                }

                if (atk.HasFlags(Flags.Charge) || atk.HasFlags(Flags.Recharge))
                {
                    return 0;
                }
                if (atk.ability.id == 56)//Protean
                {
                    atk.Type1 = atk.NextMove.type;
                    atk.Type2 = Pokemon.Type.None;
                }

                int attackStat = atk[StatTarget.AttackSpecial];
                int attackPower = atk.NextMove.power;
                int defenseStat = def[StatTarget.DefenceSpecial];

                if (atk.NextMove[35] != null)
                {
                    defenseStat = def[StatTarget.Defence];
                }

                double atkItemMod = 1;
                if (atk.item.id == 6) { atkItemMod *= 1.5; }//ChoiceSpecs
                else if (atk.item.id == 10)//LifeOrb
                {
                    atkItemMod *= 1.3;
                    if (atk.ability.id != 10 && atk.ability.id != 17)
                        atk.Hp -= (int)(atk.GetMaxHp() / 10);
                }
                if (def.item.id == 3) { atkItemMod *= 0.66; }//AssaultVest

                double stabMod = 1;
                if (atk.IsStab(atk.NextMove.type)) { stabMod *= 1.5; }//STAB

                double abilityMod = 1;
                if (atk.ability.id == 17) { abilityMod *= 1.3; }//SheerForce
                else if (atk.ability.id == 22 && atk.NextMove.power < 60) { abilityMod *= 1.5; }//Technician

                /*Meteo*/
                if (stage.Weather == WeatherType.Rain)//RainModifiers
                {
                    if (atk.NextMove.type == Pokemon.Type.Eau)
                    {
                        stabMod *= 1.5;
                    }
                    else if (atk.NextMove.type == Pokemon.Type.Feu)
                    {
                        stabMod *= 0.5;
                    }
                }
                if (stage.Weather == WeatherType.HarshSunlight)//Sunmodifiers
                {
                    if (atk.NextMove.type == Pokemon.Type.Eau)
                    {
                        stabMod *= 0.5;
                    }
                    else if (atk.NextMove.type == Pokemon.Type.Feu)
                    {
                        stabMod *= 1.5;
                    }
                }
                if (stage.Weather == WeatherType.Sandstorm)//SandModifiers
                {
                    if (def.IsStab(Pokemon.Type.Roche))
                    {
                        defenseStat = (int)(defenseStat * 1.5);
                    }
                }

                /*Terrain*/
                if (!(atk.IsStab(Pokemon.Type.Vol) || atk.ability.id == 36))
                {
                    if (stage.Terrain == TerrainType.Eletric)
                    {
                        if (atk.NextMove.type == Pokemon.Type.Electrik)
                        {
                            stabMod *= 1.5;
                        }
                        if (atk.NextMove[14] != null)
                        {
                            return 0;
                        }
                    }
                    if (stage.Terrain == TerrainType.Psychic)
                    {
                        if (atk.NextMove.type == Pokemon.Type.Psy)
                        {
                            stabMod *= 1.5;
                        }
                        if (atk.NextMove[4] != null)
                        {
                            return 0;
                        }
                    }
                }

                if (defTrainer.HasHazards(Hazards.LightScreen))//LightScreen
                    stabMod *= 0.5;

                double typeMod = Program.GetMatchup(atk.NextMove.type, def.Type1, def.Type2);

                if (def.HasFlags(Flags.Roost))//RoostEffectOnFlyingTypes
                {
                    if (def.Type1 == Pokemon.Type.Vol)
                    {
                        typeMod = Program.GetMatchup(atk.NextMove.type, def.Type2);
                    }
                    else if (def.Type2 == Pokemon.Type.Vol)
                    {
                        typeMod = Program.GetMatchup(atk.NextMove.type, def.Type1);
                    }
                }

                if (def.ability.id == 12 && atk.GetMod(StatTarget.Attack) > 0)//Unaware
                {
                    stabMod = (int)(stabMod / (1 + .5 * atk.GetMod(StatTarget.Attack)));
                }

                damageInflicted = (int)((((42 * attackStat * attackPower / defenseStat) / 50) + 2) * stabMod * typeMod * abilityMod);

                EffectGen(stage, atk, def, atkTrainer, defTrainer, ref damageInflicted); //Effects

            }

            return damageInflicted;
        }

        public static void InflictDamage(int damage, Set atk, Set def)
        {
            if (def.HasFlags(Flags.Substitute) && atk.NextMove[25] == null)//Infliger les dégâts
            {
                Program.Print(String.Format("Le clone prend les dégâts à la place de {0}", def.GetName()));
                def.RemoveFlags(Flags.Substitute);
            }
            else
            {
                def.Hp -= damage;
            }
            if (atk.item.id == 10 && damage != 0)
            {
                atk.Hp -= (int)(atk.Hp * 0.1);
            }
            if (atk.NextMove.sps == Sps.Physic && def.item.id == 11)
            {
                atk.Hp -= (int)(atk.Hp * 0.125);
            }
            if (atk.NextMove.sps == Sps.Physic && def.ability.id == 53)
            {
                atk.Hp -= (int)(atk.Hp * 0.125);
            }
        }

        public static void EffectGen(Stage stage, Set atk, Set def, Trainer atkTrainer, Trainer defTrainer, ref int damageInflicted)
        {
            SideEffect(atk.NextMove, 2, ref def, Status.Burn);//Burn

            SideEffect(atk.NextMove, 3, ref def, Flags.Flinch);//Flinch

            SideEffect(atk.NextMove, 5, ref def, Flags.MagmaStorm);//MagmaStorm

            if (atk.NextMove[7] != null)//AtkBoost
            {
                if (Roll(atk.NextMove, 7))
                {
                    atk[StatTarget.Attack] = atk.NextMove[7].Value.value;
                }
            }

            if (atk.NextMove[8] != null)//DefBoost
            {
                if (Roll(atk.NextMove, 8))
                {
                    atk[StatTarget.Defence] = atk.NextMove[8].Value.value;
                }
            }
            if (atk.NextMove[9] != null)//SpaBoost
            {
                if (Roll(atk.NextMove, 9))
                {
                    atk[StatTarget.AttackSpecial] = atk.NextMove[9].Value.value;
                }
            }
            if (atk.NextMove[10] != null)//SpdBoost
            {
                if (Roll(atk.NextMove, 10))
                {
                    atk[StatTarget.DefenceSpecial] = atk.NextMove[10].Value.value;
                }
            }
            if (atk.NextMove[11] != null)//SpeBoost
            {
                if (Roll(atk.NextMove, 11))
                {
                    atk[StatTarget.Speed] = atk.NextMove[11].Value.value;
                }
            }

            if (atk.NextMove[19] != null)//MultiStrike
            {
                int roll = Program.RngNext(1, 101);
                if (roll >= 75)
                {
                    Program.Print("Touché 2 fois");
                    damageInflicted *= 2;
                }
                else if (roll >= 50)
                {
                    Program.Print("Touché 3 fois");
                    damageInflicted *= 3;
                }
                else if (roll >= 25)
                {
                    Program.Print("Touché 4 fois");
                    damageInflicted *= 4;
                }
                else
                {
                    Program.Print("Touché 5 fois");
                    damageInflicted *= 5;
                }
            }

            SideEffect(atk.NextMove, 21, ref def, Status.Paralysis);//Paralysis

            SideEffect(atk.NextMove, 5, ref def, Flags.Recharge);//Recharge

            if (atk.NextMove[26] != null)
            {
                atkTrainer.RemoveHazards();
            }

            if (atk.NextMove[7] != null)//AdvAtkBoost
            {
                if (Roll(atk.NextMove, 7))
                {
                    atk[StatTarget.Attack] = def.NextMove[7].Value.value;
                }
            }

            if (atk.NextMove[8] != null)//AdvDefBoost
            {
                if (Roll(atk.NextMove, 8))
                {
                    atk[StatTarget.Defence] = def.NextMove[8].Value.value;
                }
            }
            if (atk.NextMove[9] != null)//AdvSpaBoost
            {
                if (Roll(atk.NextMove, 9))
                {
                    atk[StatTarget.AttackSpecial] = def.NextMove[9].Value.value;
                }
            }
            if (atk.NextMove[10] != null)//AdvSpdBoost
            {
                if (Roll(atk.NextMove, 10))
                {
                    atk[StatTarget.DefenceSpecial] = def.NextMove[10].Value.value;
                }
            }
            if (atk.NextMove[11] != null)//AdvSpeBoost
            {
                if (Roll(atk.NextMove, 11))
                {
                    atk[StatTarget.Speed] = def.NextMove[11].Value.value;
                }
            }

            SideEffect(atk.NextMove, 33, ref def, Flags.Confusion);//Confusion

            if (atk.NextMove[40] != null)//KnockOff
            {
                if (def.item.id != 0)
                {
                    damageInflicted = (int)(1.5 * damageInflicted);
                    def.RemoveItem();
                }
            }

            SideEffect(atk.NextMove, 41, ref def, Status.Poison);//Poison

            SideEffect(atk.NextMove, 42, ref def, Status.BadlyPoisoned);//Toxic

            if (atk.NextMove[43] != null)//Acrobatics
            {
                if (atk.item == null)
                {
                    damageInflicted = (int)(damageInflicted * 1.5);
                }
            }
        }

        public static int CritGen(int damageInflicted, Set atk) //Crit
        {
            int rdNumber = Program.RngNext(1, 101);
            if (atk.NextMove[16] != null)
            {
                if (rdNumber <= 12)
                {
                    damageInflicted = (int)(damageInflicted * 1.5);
                    Program.Print("Coup critique !");
                }
            }
            else
            {
                if (rdNumber <= 4)
                {
                    damageInflicted = (int)(damageInflicted * 1.5);
                    Program.Print("Coup critique !");
                }
            }
            return damageInflicted;
        }

        public static bool Roll(Move attack, int idEffect)
        {
            int roll = Program.RngNext(1, 101);
            return (attack[idEffect].Value.percent <= roll);
        }

        public static void SideEffect(Move attack, int idEffect, ref Set defPoke, Status status)
        {
            if (attack[idEffect] != null)
            {
                if (Roll(attack, idEffect))
                {
                    defPoke.Status = status;
                }
            }
        }

        public static void SideEffect(Move attack, int idEffect, ref Set defPoke, Flags flag)
        {
            if (attack[idEffect] != null)
            {
                if (Roll(attack, idEffect))
                {
                    defPoke.AddFlags(flag);
                }
            }
        }

        public static bool DefiantActive(Set before, Set after)
        {
            if (before.GetMod(StatTarget.Attack) > after.GetMod(StatTarget.Attack))
            {
                return true;
            }
            if (before.GetMod(StatTarget.Defence) > after.GetMod(StatTarget.Defence))
            {
                return true;
            }
            if (before.GetMod(StatTarget.AttackSpecial) > after.GetMod(StatTarget.AttackSpecial))
            {
                return true;
            }
            if (before.GetMod(StatTarget.DefenceSpecial) > after.GetMod(StatTarget.DefenceSpecial))
            {
                return true;
            }
            if (before.GetMod(StatTarget.Speed) > after.GetMod(StatTarget.Speed))
            {
                return true;
            }
            return false;
        }
    }
}
