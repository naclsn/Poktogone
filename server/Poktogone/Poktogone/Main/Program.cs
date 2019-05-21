using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using Poktogone.Pokemon;
using Poktogone.Battle;

namespace Poktogone.Main
{
    class Program
    {
        static private SqlHelper dbo; // Ued to query to the local Pokemon DB (the .mdf file).
        static private bool isFromCmd; // Used to know weather called with or without arguments.
        static private Random rng; // Grlobal RNG, private; to get RNG, usethe `RngNext` familly of functions.

        /// <summary>
        /// Play a battle between the trainers defined by thir names and sets if given through args,
        /// else ask for the names through stdin and use randomly choosen sets from the database.
        /// If the battle succed to start, process to play a game: get players commands, play turn, resets status.
        /// </summary>
        /// <param name="args">`Poktogone.exe nameP1 teamP1 nameP2 teamP2 [--dbo fileName] [--rng seed] [--dmc (damage calculator args)] [--log [fileName]]`.</param>
        /// <returns>0 if success, else last known <see cref="BattleState"/> (as an int).</returns>
        static int Main(String[] args)
        {
            Program.dbo = new SqlHelper();
            Program.isFromCmd = 0 < args.Length;
            Program.rng = new Random();

            // TODO: args parse (meh..)

            Program.Log("info", "Connecting to sqllocaldb");
            Program.dbo.Connect(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Database.mdf"));
            Program.Log("info", "\tConnected!");

            Trainer P1, P2;
            Battle.Battle battle;

            Program.Log("info", "Loading players' sets");
            if (Program.isFromCmd)
            {
                P1 = new Trainer(args[0], ParseSets(args[1]));
                P2 = new Trainer(args[2], ParseSets(args[3]));
            }
            else
            {
                P1 = new Trainer(Program.Input("Player 1 name: "), ParseSets($"{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1}"));
                P2 = new Trainer(Program.Input("Player 2 name: "), ParseSets($"{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1};{Program.rng.Next(121) + 1}"));
            }
            Program.Log("info", "\tLoaded!");

            battle = new Battle.Battle(P1, P2);
            Program.Println(battle);

            Program.Input("Press Enter to start... ");
            Program.ConsoleClear();

            if (battle.Start())
            {
                do
                {
                    Program.Println(battle);
                    int code = -1;
                    try
                    {
                        code = battle.InputCommand(int.Parse(Program.Input("Your player num: ")), Program.Input("Your command (attack / switch): "));
                    }
                    catch (FormatException)
                    {
                        Program.Print("This is not a player number... ");
                    }

                    if (code < 0)
                        Program.Println("Wrong player number! (nice try tho...)");
                    else if (code == 0)
                        Program.Println("Wrong command name! (or typo...)");

                    Program.Input("Press Enter to continue... ");
                    Program.ConsoleClear();
                }
                while (battle.State != BattleState.VictoryP1 || battle.State != BattleState.VictoryP2);
            }
            else
            {
                Program.Println($"Couln't start battle... Last known state: {battle.State}");
                return (int)battle.State;
            }
            
            Program.Println(battle.State);
            return 0;
        }

        /// <summary>
        /// Prints to stdout using the <seealso cref="Console"/>.
        /// </summary>
        /// <param name="o">Object to print.</param>
        public static void Print(Object o)
        {
            Console.Write(o.ToString());
        }

        /// <summary>
        /// Prints to stdout using the <seealso cref="Console"/>, then line feeds.
        /// </summary>
        /// <param name="o">Object to print.</param>
        public static void Println(Object o)
        {
            Program.Print(o.ToString() + "\n");
        }

        /// <summary>
        /// Line feeds.
        /// </summary>
        public static void Println()
        {
            Program.Println("");
        }

        /// <summary>
        /// Waits for an input from stdin using the <seealso cref="Console"/>.
        /// </summary>
        /// <returns>The String read.</returns>
        public static String Input()
        {
            Program.PhpMessage("int", "waiting for user input");
            return Console.ReadLine();
        }

        /// <summary>
        /// Prints to stdout then waits for an input from stdin using the <seealso cref="Console"/>.
        /// </summary>
        /// <param name="o">Object to print.</param>
        /// <returns>The String read.</returns>
        public static String Input(Object o)
        {
            Program.Print(o);
            return Program.Input();
        }

        /// <summary>
        /// Clear the <seealso cref="Console"/>.
        /// </summary>
        /// <remarks>Only if no args where proviede at start (<seealso cref="Program.isFromCmd"/>).</remarks>
        public static void ConsoleClear()
        {
            if (!Program.isFromCmd)
                Console.Clear();
            Program.PhpMessage("clc", "clearing buffer");
        }

        /// <summary>
        /// Used to send signals to the PHP scripts (see web-based).
        /// </summary>
        /// <param name="signal">Signal identifier (3 chars).</param>
        /// <param name="description">Optionnal description of the signal.</param>
        /// <remarks>(I don't want to setup any more pipes.)</remarks>
        public static void PhpMessage(String signal, String description)
        {
            Console.Error.Write($"{signal}/{description} (this is not an error)\n");
        }

        /// <summary>
        /// Prints to the log (can be specified through the args, default to the <seealso cref="Console"/>).
        /// </summary>
        /// <param name="tag">Tag to display before the message</param>
        /// <param name="c">Text to display </param>
        public static void Log(String tag, String c)
        {
            // TOD: use the specified output (should be a file), if any.
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm")}] {tag}: {c}");
        }

        /// <summary>
        /// Return the next pseudo random int value, from 0 (included).
        /// </summary>
        /// <param name="maxValue">Max value (excluded).</param>
        /// <returns>A pseudo random, uniformly distributed, int.</returns>
        public static int RngNext(int maxValue)
        {
            return Program.rng.Next(maxValue);
        }

        /// <summary>
        /// Return the next pseudo random int value.
        /// </summary>
        /// <param name="minValue">Min value (included).</param>
        /// <param name="maxValue">Max value (excluded).</param>
        /// <returns>A pseudo random, uniformly distributed, int.</returns>
        public static int RngNext(int minValue, int maxValue)
        {
            return Program.rng.Next(minValue, maxValue);
        }

        /// <summary>
        /// Parse and load from database the 3 sets specified by the argument.
        /// </summary>
        /// <param name="arg">"[setId1];[setId2];[setId3]"</param>
        /// <param name="sep">Separator, use ';' by default</param>
        /// <returns>Return a list of 3 sets.</returns>
        static Set[] ParseSets(String arg, char sep = ';')
        {
            Set[] r = new Set[3];
            int k = 0;

            foreach (String id in arg.Split(sep))
                r[k++] = Set.FromDB(Program.dbo, int.Parse(id));

            return r;
        }

        public static float GetMatchup(Pokemon.Type typeAtk, Pokemon.Type typeDef)
        {
            if (typeDef == Pokemon.Type.None) return 1;
            return float.Parse(dbo.Select("matchups", new Where("type_atk", (int)typeAtk).And("type_def", (int)typeDef), "coef")[0]["coef"]);
        }

        public static float GetMatchup(Pokemon.Type typeAtk, Pokemon.Type typeDef1, Pokemon.Type typeDef2)
        {
            return Program.GetMatchup(typeAtk, typeDef1) * (typeDef2 == Pokemon.Type.None ? 1 : Program.GetMatchup(typeAtk, typeDef2));
        }

        /// <summary>
        /// Calculate and apply the damage of a move from an attacking pokémon, to the defending pokémon of the defending trainer,
        /// while accounding for the stage's weather, terrain [and other...].
        /// </summary>
        /// <param name="stage">Context for the actions.</param>
        /// <param name="atk">Attacking pokémon.</param>
        /// <param name="def">Defending pokémon.</param>
        /// <param name="defTrainer">Trainer of the defending pokémon.</param>
        /// <returns>The damage inflicted, in percents.</returns>
        public static int DamageCalculator(Stage stage, Set atk, Set def,Trainer atkTrainer, Trainer defTrainer)
        {
            int damageInflicted = 0;
            if (atk.HasFlags(Flags.Flinch)) //Flinch
            {
                return 0;
            }

            //====================SUPPORT=====================//

            if (atk.NextMove.sps == Sps.Stat) //Support
            {
                if (atk.NextMove[1] != null)//Climat
                {
                    stage.Weather = (WeatherType) atk.NextMove[1].Value.value;
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
                    atk.Hp += atk.Hp * atk.NextMove[13].Value.value /100;
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
                if (atk.NextMove[18] != null)//Leechseed
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
                    atk.Hp -= (int) (atk.GetMaxHp() * 0.25);
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
                else if (atk.item.id == 10) { atkItemMod *= 1.3; }//LifeOrb

                double stabMod = 1;
                if (atk.IsStab(atk.NextMove.type)) { stabMod *= 1.5; }//STAB

                double abilityMod = 1;
                if (atk.ability.id == 17) { abilityMod *= 1.3; }//SheerForce
                else if (atk.ability.id == 20) { abilityMod *= 2; }//HugePower
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


                double typeMod = GetMatchup(atk.NextMove.type, def.Type1, def.Type2);

                if (typeMod == 0)//Immunité
                {
                    if(atk.NextMove[27] != null)//HighJumpKick
                    {
                        atk.Hp -= atk.GetMaxHp() / 2;
                    }
                    return 0;
                }

                if (atk.Status == Status.Burn) { typeMod *= 1 / 2; }//Burn

                if(atk.NextMove[37] != null && stage.Weather == WeatherType.Rain)//Cas particulier de Fatal-foudre et Blizzard
                {
                    damageInflicted = (int)((((42 * attackStat * attackPower / defenseStat) / 50) + 2) * stabMod * typeMod * abilityMod * atkItemMod);
                }
                else if (RngNext(1, 100) <= atk.NextMove.accuracy)//Hit
                {
                    damageInflicted = (int)((((42 * attackStat * attackPower / defenseStat) / 50) + 2) * stabMod * typeMod * abilityMod * atkItemMod);
                }
                else//Miss
                {
                    if (atk.NextMove[27] != null)//HighJumpKick
                    {
                        atk.Hp -= atk.GetMaxHp() / 2;
                    }
                    Print(string.Format("{0} Rate son attaque", atk.GetName()));
                    return 0;
                }

                if (atk.ability.id == 17)//SheerForce
                {
                    return damageInflicted;
                }

                EffectGen(stage, atk, def, atkTrainer, defTrainer, damageInflicted); //Effects

                if (def.item.id == 11)//RockyHelmet
                {
                    atk.Hp -= (int)(atk.GetMaxHp() / 8);
                }


            }

            //====================SPECIAL=====================//

            if (atk.NextMove.sps == Sps.Special) //Special
            {
                if (atk.ability.id == 56)//Protean
                {
                    atk.Type1 = atk.NextMove.type;
                    atk.Type2 = Pokemon.Type.None;
                }

                int attackStat = atk[StatTarget.AttackSpecial];
                int attackPower = atk.NextMove.power;
                int defenseStat = def[StatTarget.DefenceSpecial];

                if(atk.NextMove[35] != null)
                {
                    defenseStat = def[StatTarget.Defence];
                }

                double atkItemMod = 1;
                if (atk.item.id == 6) { atkItemMod *= 1.5; }//ChoiceSpecs
                else if (atk.item.id == 10) { atkItemMod *= 1.3; }//LifeOrb

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

                double typeMod = GetMatchup(atk.NextMove.type, def.Type1, def.Type2);

                damageInflicted = (int)((((42 * attackStat * attackPower / defenseStat) / 50) + 2) * stabMod * typeMod * abilityMod);

                EffectGen(stage, atk, def, atkTrainer, defTrainer, damageInflicted); //Effects

            }

            return damageInflicted;
        }

        public static void InflictDamage(int damage, Set atk, Set def)
        {
            if (def.HasFlags(Flags.Substitute) && atk.NextMove[25] == null)//Infliger les dégâts
            {
                Print(String.Format("Le clone prend les dégâts à la place de {0}", def.GetName()));
                def.RemoveFlags(Flags.Substitute);
            }
            else
            {
                def.Hp -= damage;
            }
            if(atk.item.id == 10 && damage != 0)
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

        public static void EffectGen(Stage stage, Set atk, Set def, Trainer atkTrainer, Trainer defTrainer, int damageInflicted)
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
                int roll = RngNext(1, 101);
                if (roll >= 75)
                {
                    Print("Touché 2 fois");
                    damageInflicted *= 2;
                }
                else if (roll >= 50)
                {
                    Print("Touché 3 fois");
                    damageInflicted *= 3;
                }
                else if (roll >= 25)
                {
                    Print("Touché 4 fois");
                    damageInflicted *= 4;
                }
                else
                {
                    Print("Touché 5 fois");
                    damageInflicted *= 5;
                }
            }

            SideEffect(atk.NextMove, 21, ref def, Status.Paralysis);//Paralysis

            if (atk.NextMove[22] != null)//Pivotage
            {
                /*jesaispas*/
            }

            SideEffect(atk.NextMove, 23, ref def, Flags.Recharge);

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

            SideEffect(atk.NextMove, 34, ref def, Flags.Colere);//Outrage

            if (atk.NextMove[40] != null)//KnockOff
            {
                if (def.item.id != 0)
                {
                    def.item.Remove();
                    damageInflicted = (int)(1.5 * damageInflicted);
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
                    Print("Coup critique !");
                } 
            }
            else
            {
                if (rdNumber <= 4)
                {
                    damageInflicted = (int)(damageInflicted * 1.5);
                    Print("Coup critique !");
                }
            }
            return damageInflicted;
        }

        public static bool Roll(Move attack, int idEffect)
        {
            int roll = Program.RngNext(1, 101);
            return (attack[idEffect].Value.percent <= roll);
        }

        public static void SideEffect(Move attack, int idEffect,ref Set defPoke, Status status)
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


    }
}
