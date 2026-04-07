using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class Weapon
{
    public string Name { get; set; }
    public int AttackBonus { get; set; }
    public string Description { get; set; }

    public Weapon(string name, int attackBonus, string description = "")
    {
        Name = name;
        AttackBonus = attackBonus;
        Description = description;
    }
}

public class Consumable
{
    public string Name { get; set; }
    public int Hp { get; set; }
    public string Description { get; set; }

    public Consumable(string name, int hpRestore, string description = "")
    {
        Name = name;
        Hp = hpRestore;
        Description = description;
    }
}

public class PlayerData
{
    public string UserName { get; set; }
    public int FloorCount { get; set; }
    public int Level { get; set; }
    public int BaseAttack { get; set; }
    public int Max_Base_HP { get; set; }
    public int Base_HP { get; set; }
    public int CurrentHP { get; set; }
    public int Experience { get; set; }
    public int ExperienceToNextLevel { get; set; }
    public List<Weapon> Inventory { get; set; } = new();
    public List<Consumable> Potions { get; set; } = new();
    public bool OverlordDefeated { get; set; } = false;

    public int GetTotalAttack()
    {
        return BaseAttack + Inventory.Sum(w => w.AttackBonus);
    }
        public int GetTotalHp()
    {
        return Base_HP + Potions.Sum(p => p.Hp);
    }
        public int GetTotalMaxHp()
    {
        return Max_Base_HP + Potions.Sum(p => p.Hp);
    }
    public string GetWeaponSummary()
    {
        if (Inventory.Count == 0)
            return "None";

        return string.Join(", ", Inventory.Select(w => $"{w.Name}(+{w.AttackBonus})"));
    }

    public string GetPotionSummary()
    {
        if (Potions.Count == 0)
            return "None";

        return string.Join(", ", Potions.GroupBy(p => p.Name)
            .Select(g => $"{g.Key} x{g.Count()}"));
    }
}

public class Enemy
{
    public string Name { get; set; }
    public int HP { get; set; }
    public int Attack { get; set; }

    public Enemy(string name, int hp, int attack)
    {
        Name = name;
        HP = hp;
        Attack = attack;
    }
}

class Program
{
    static void Main(string[] args)
    {
        PlayerData player = new PlayerData {
            UserName = "Hero",
            Level = 1,
            FloorCount = 0,
            BaseAttack = 3,
            Max_Base_HP = 20,
            Base_HP = 20,
            CurrentHP = 20,
            Experience = 0,
            ExperienceToNextLevel = 10,
            Inventory = new List<Weapon>
            {
                new Weapon("Disgraced Sword", 3, "A rusty blade with a tarnished reputation."),
            }
        };
        RunMap(player);
    }
    static char[,] GenerateMap(int width, int height)
{
    var rand = new Random();
    char[,] map = new char[height, width];

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
            {
                map[y, x] = '#'; // border wall
            }
            else
            {
                int roll = rand.Next(100);
                if (roll < 70)
                    map[y, x] = '.';   // floor
                else if (roll < 85)
                    map[y, x] = 'E';   // encounter
                else if (roll < 95)
                    map[y, x] = 'T';   // treasure
                else
                    map[y, x] = '.';   // floor (extra chance for open space)
            }
        }
    }

    return map;
}
    static char[,] GenerateMapWithTracking(int width, int height, out int encounterCount)
    {
        var rand = new Random();
        char[,] map = new char[height, width];
        encounterCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                {
                    map[y, x] = '#'; // border wall
                }
                else
                {
                    int roll = rand.Next(100);
                    if (roll < 70)
                    {
                        map[y, x] = '.';   // floor
                    }
                    else if (roll < 85)
                    {
                        map[y, x] = 'E';   // encounter
                        encounterCount++;
                    }
                    else if (roll < 95)
                    {
                        map[y, x] = 'T';   // treasure
                    }
                    else
                    {
                        map[y, x] = '.';   // floor (extra chance for open space)
                    }
                }
            }
        }

        return map;
    }

    static Weapon GenerateRandomWeapon()
    {
        var rand = new Random();
        var aspects = new[]
        {
            new { Name = "Ignited ", Bonus = 2 },
            new { Name = "Frozen ", Bonus = 1 },
            new { Name = "Disgraced ", Bonus = 0 },
            new { Name = "Electric ", Bonus = 4 },
            new { Name = "Legendary ", Bonus = 5 },
            new { Name = "Shadow ", Bonus = 3 }
        };

        var weapons = new[]
        {
            new { Name = "Spear", Base = 3 },
            new { Name = "Sword", Base = 2 },
            new { Name = "Battle Axe", Base = 4 },
            new { Name = "Claymore", Base = 5 },
            new { Name = "Dagger", Base = 1 }
        };

        var aspect = aspects[rand.Next(aspects.Length)];
        var weapon = weapons[rand.Next(weapons.Length)];

        string name = aspect.Name + weapon.Name;
        int attack = weapon.Base + aspect.Bonus;

Weapon result = new Weapon(name, attack, $"A {weapon.Name.ToLower()} imbued with {aspect.Name.ToLower().Trim()}power, granting +{attack} attack.");
        return result;
    }

    static Consumable GenerateRandomPotion()
    {
        var rand = new Random();
        var potionTemplates = new[]
        {
            new { Name = "Minor Health Talisman", Hp = 15, Description = "Adds a small amount of HP." },
            new { Name = "Health Talisman", Hp = 30, Description = "Adds a moderate amount of HP." },
            new { Name = "Greater Health Talisman", Hp = 50, Description = "Adds a large amount of HP." },
            new { Name = "Charm of Vitality", Hp = 100, Description = "A magical elixir that adds a very large chunk of HP." }
        };

        var template = potionTemplates[rand.Next(potionTemplates.Length)];
        return new Consumable(template.Name, template.Hp, template.Description);
    }
    static void RunMap(PlayerData player)
    {
        int floorNumber = 1;
        int encountersCleared = 0;
        int totalEncounters = 0;

        char[,] map = GenerateMapWithTracking(9, 9, out totalEncounters);
        int playerX = 4;
        int playerY = 4;

        while (true)
        {
            Console.Clear();
            DrawMap(map, playerX, playerY);
            Console.WriteLine($"Floor {floorNumber} | Encounters: {encountersCleared}/{totalEncounters} | Weapons: {player.GetWeaponSummary()} | Total ATK: {player.GetTotalAttack()}");
            Console.WriteLine("Move: W A S D / I inventory / Q to quit");

            string input = Console.ReadLine()?.ToUpper() ?? "";
            if (input == "Q") break;
            if (input == "I")
            {
                ShowInventory(player);
                continue;
            }

            int dx = 0;
            int dy = 0;

            if (input == "W") dy = -1;
            else if (input == "S") dy = 1;
            else if (input == "A") dx = -1;
            else if (input == "D") dx = 1;
            else continue;

            int newX = playerX + dx;
            int newY = playerY + dy;

            if (CanMove(map, newX, newY))
            {
                playerX = newX;
                playerY = newY;

                char tile = map[playerY, playerX];

                if (tile == 'E')
                {
                    map[playerY, playerX] = '.'; // Clear the encounter
                    encountersCleared++;
                    Encounter(player);

                    // Check if floor is cleared
                    if (encountersCleared >= totalEncounters)
                    {
                        Console.Clear();
                        Console.WriteLine($"🎉 Floor {floorNumber} cleared!");
                        Console.WriteLine("A boss is approaching...");
                        Thread.Sleep(2000);

                        BossEncounter(player, floorNumber);

                        Console.Clear();
                        Console.WriteLine($"Boss defeated! Generating next floor...");
                        Thread.Sleep(2000);

                        floorNumber++;
                        encountersCleared = 0;
                        map = GenerateMapWithTracking(9, 9, out totalEncounters);
                        playerX = 4;
                        playerY = 4;
                    }
                }
                else if (tile == 'T')
                {
                    map[playerY, playerX] = '.'; // Clear the treasure
                    var rand = new Random();
                    var newWeapon = GenerateRandomWeapon();
                    var newPotion = GenerateRandomPotion();
                    player.Inventory.Add(newWeapon);
                    player.Potions.Add(newPotion);
                    Console.WriteLine($"You found a {newWeapon.Name}!");
                    Console.WriteLine($"You found a {newPotion.Name}!");
                    Thread.Sleep(1500);
                }
            }
        }
    }

static void DrawMap(char[,] map, int playerX, int playerY)
{
    int rows = map.GetLength(0);
    int cols = map.GetLength(1);

    for (int y = 0; y < rows; y++)
    {
        for (int x = 0; x < cols; x++)
        {
            if (x == playerX && y == playerY)
                Console.Write('P');
            else
                Console.Write(map[y, x]);
        }
        Console.WriteLine();
    }
}

static bool CanMove(char[,] map, int x, int y)
{
    int rows = map.GetLength(0);
    int cols = map.GetLength(1);

    if (x < 0 || x >= cols || y < 0 || y >= rows)
        return false;

    return map[y, x] != '#'; // walls are '#'
}

    static void ShowInventory(PlayerData player)
    {
        bool viewingInventory = true;
        while (viewingInventory)
        {
            Console.Clear();
            Console.WriteLine("=== INVENTORY ===");
            Console.WriteLine("\nWEAPONS:");
            if (player.Inventory.Count == 0)
                Console.WriteLine("  (empty)");
            else
                for (int i = 0; i < player.Inventory.Count; i++)
                    Console.WriteLine($"  {i + 1}. {player.Inventory[i].Name} (+{player.Inventory[i].AttackBonus})");

            Console.WriteLine("\nTALISMANS:");
            if (player.Potions.Count == 0)
                Console.WriteLine("  (empty)");
            else
            {
                var potionGroups = player.Potions.GroupBy(p => p.Name).ToList();
                for (int i = 0; i < potionGroups.Count; i++)
                {
                    var group = potionGroups[i];
                    Console.WriteLine($"  {i + 1}. {group.Key} x{group.Count()} (Adds +{group.First().Hp} HP)");
                }
            }

            Console.WriteLine($"\nBase Attack: {player.BaseAttack}");
            Console.WriteLine($"Total Attack: {player.GetTotalAttack()}");
            Console.WriteLine($"HP: {player.GetTotalHp()}/{player.GetTotalMaxHp()}");
            Console.WriteLine("\nPress I to return to the map...");
            Console.ReadKey();
        }
    }

    static void Encounter(PlayerData player)
    {
        int totalHp = player.GetTotalHp();
        int totalMaxHp = player.GetTotalMaxHp();
        int totalAttack = player.GetTotalAttack();
        Random rand = new Random();
        int healsRemaining = 3;

        var enemyTemplates = new[]
        {
            new { Name = "Goblin", BaseHP = 5, BaseAttack = 2 },
            new { Name = "Orc", BaseHP = 12, BaseAttack = 4 },
            new { Name = "Skeleton", BaseHP = 8, BaseAttack = 3 },
            new { Name = "Troll", BaseHP = 20, BaseAttack = 6 },
            new { Name = "Dragon", BaseHP = 35, BaseAttack = 10 }
        };

        var template = enemyTemplates[rand.Next(enemyTemplates.Length)];
        int levelScaling = player.Level / 3;
        int hpVariation = rand.Next(-2, 3);
        int attackVariation = rand.Next(-1, 2);

        int enemyHP = Math.Max(1, template.BaseHP + hpVariation + (levelScaling * 3));
        int enemyAttack = Math.Max(1, template.BaseAttack + attackVariation + levelScaling);

        Enemy enemy = new Enemy(template.Name, enemyHP, enemyAttack);
        Console.WriteLine($"A wild {enemy.Name} appears!");

        while (enemy.HP > 0 && totalHp > 0)
        {
            Console.WriteLine($"\nYour HP: LV {player.Level} ATK {totalAttack} {totalHp}/{totalMaxHp} | EXP: {player.Experience}/{player.ExperienceToNextLevel} | {enemy.Name} HP: {enemy.HP}");
            Console.WriteLine($"Active weapons: {player.GetWeaponSummary()}");
            Console.WriteLine("1. Attack");
            Console.WriteLine($"2. Heal ({healsRemaining}/3 remaining)");
            Console.WriteLine("3. Run");
            Console.Write("> ");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                enemy.HP -= totalAttack;
                Console.WriteLine($"You hit the {enemy.Name} for {totalAttack} damage!");

                if (enemy.HP > 0)
                {
                    totalHp -= enemy.Attack;
                    Console.WriteLine($"{enemy.Name} hits you for {enemy.Attack} damage!");
                }
            }
            else if (choice == "2")
            {
                if (healsRemaining > 0)
                {
                    totalHp = player.GetTotalHp();
                    totalMaxHp = player.GetTotalMaxHp();
                    totalAttack = player.GetTotalAttack();
                    int healAmount = 10 + (totalAttack * 2);
                    int oldHP = totalHp;
                    totalHp = Math.Min(totalHp + healAmount, totalMaxHp);
                    int actualHealed = totalHp - oldHP;
                    
                    Console.WriteLine($"You healed for {actualHealed} HP!");
                    healsRemaining--;
                    
                    if (enemy.HP > 0)
                    {
                        totalHp -= enemy.Attack;
                        Console.WriteLine($"{enemy.Name} hits you for {enemy.Attack} damage!");
                    }
                }
                else
                {
                    Console.WriteLine("You are out of heals for this encounter!");
                }
            }
            else if (choice == "3")
            {
                Console.WriteLine("You ran away!");
                return;
            }
            else
            {
                Console.WriteLine("Invalid choice, try again.");
            }
        }

        if (player.GetTotalHp() <= 0)
        {
            Console.WriteLine("\nYou were defeated...");
            player.CurrentHP = player.GetTotalMaxHp();
        }
        else
        {
            if (enemy.HP <= 0){
                enemy.HP = 0; // Ensure HP doesn't go negative for display purposes
            }
            Console.WriteLine($"\nYou defeated the {enemy.Name}!");
            int expGained = CalculateExperience(enemy, player.Level);
            player.Experience += expGained;
            Console.WriteLine($"You gained {expGained} experience!");
            Random rng = new Random();
            var newWeapon = GenerateRandomWeapon();
            player.Inventory.Add(newWeapon);

            // Random chance to drop a potion
            if (rng.Next(100) < 40) // 40% chance
            {
                var newPotion = GenerateRandomPotion();
                player.Potions.Add(newPotion);
                Console.WriteLine($"Enemy dropped {newPotion.Name}!");
            }

            CheckLevelUp(player);
        }
    }

    static int CalculateExperience(Enemy enemy, int playerLevel)
    {
        int baseExp = enemy.HP + (enemy.Attack * 2);
        int levelDiff = playerLevel - 1;
        double levelModifier = Math.Max(0.5, 1.0 + (levelDiff * 0.1));
        return (int)(baseExp * levelModifier);
    }

    static void CheckLevelUp(PlayerData player)
    {
        while (player.Experience >= player.ExperienceToNextLevel && player.Level < 50)
        {
            player.Experience -= player.ExperienceToNextLevel;
            PerformLevelUp(player);
        }
    }

    static void BossEncounter(PlayerData player, int floorNumber)
    {
        var bosses = new[]
        {
            new { Name = "Goblin King", BaseHP = 70, BaseAttack = 10, BonusExp = 80 },
            new { Name = "Troll Warlord", BaseHP = 95, BaseAttack = 14, BonusExp = 120 },
            new { Name = "Dragon Lord", BaseHP = 130, BaseAttack = 18, BonusExp = 180 },
            new { Name = "Necromancer", BaseHP = 155, BaseAttack = 22, BonusExp = 220 },
            new { Name = "Demon Overlord", BaseHP = 190, BaseAttack = 26, BonusExp = 300 }
        };

        int bossIndex = Math.Min(floorNumber - 1, bosses.Length - 1);
        var bossTemplate = bosses[bossIndex];
        int bossHP = bossTemplate.BaseHP + (player.Level * 5);
        int bossAttack = bossTemplate.BaseAttack + (floorNumber * 2);
        Enemy boss = new Enemy(bossTemplate.Name, bossHP, bossAttack);

        Console.WriteLine($"A boss appears: {boss.Name}! HP: {boss.HP}, ATK: {boss.Attack}");
        Thread.Sleep(1500);

        int healsRemaining = 3;
            int totalMaxHp = player.GetTotalMaxHp();
            int totalAttack = player.GetTotalAttack();
        while (boss.HP > 0 && player.CurrentHP > 0)
        {
            Console.WriteLine($"\nBoss {boss.Name}: {boss.HP} HP | You: {player.CurrentHP}/{totalMaxHp} | ATK: {totalAttack}");
            Console.WriteLine("1. Attack");
            Console.WriteLine($"2. Heal ({healsRemaining}/3 remaining)");
            Console.WriteLine("3. Run");
            Console.Write("> ");

            string choice = Console.ReadLine();
            if (choice == "1")
            {
                boss.HP -= totalAttack;
                Console.WriteLine($"You hit {boss.Name} for {totalAttack} damage!");
            }
            else if (choice == "2")
            {
                if (healsRemaining > 0)
                {
                    int healAmount = 10 + (totalAttack * 2);
                    int oldHP = player.CurrentHP;
                    player.CurrentHP = Math.Min(player.CurrentHP + healAmount, totalMaxHp);
                    int healed = player.CurrentHP - oldHP;
                    Console.WriteLine($"You healed for {healed} HP!");
                    healsRemaining--;
                }
                else
                {
                    Console.WriteLine("You are out of heals for the boss fight!");
                }
            }
            else if (choice == "3")
            {
                Console.WriteLine("You can't run from a boss!");
                continue;
            }
            else
            {
                Console.WriteLine("Invalid choice, try again.");
                continue;
            }

            if (boss.HP > 0)
            {
                player.CurrentHP -= boss.Attack;
                Console.WriteLine($"{boss.Name} hits you for {boss.Attack} damage!");
            }
        }

        if (player.CurrentHP <= 0)
        {
            Console.WriteLine("\nYou were defeated by the boss...");
            player.CurrentHP = player.GetTotalMaxHp();
        }
        else
        {
            Console.WriteLine($"\nYou defeated {boss.Name}!");
            player.Experience += bossTemplate.BonusExp;
            Console.WriteLine($"You gained {bossTemplate.BonusExp} bonus experience!");

            var bossWeapon = GenerateRandomWeapon();
            player.Inventory.Add(bossWeapon);
            Console.WriteLine($"Boss dropped a {bossWeapon.Name}!");

            if (new Random().Next(100) < 60)
            {
                var bossPotion = GenerateRandomPotion();
                player.Potions.Add(bossPotion);
                Console.WriteLine($"You also found {bossPotion.Name}!");
            }
            if (floorNumber == 5)
            {
                player.OverlordDefeated = true;
                Console.WriteLine("You have defeated the Demon Overlord and completed the game!");
            }
            CheckLevelUp(player);
            Thread.Sleep(2000);
        }
    }

    static void PerformLevelUp(PlayerData player)
    {
        player.Level++;
        
        int attackIncrease = 1 + (player.Level / 5);
        int hpIncrease = 3 + (player.Level / 3);
        
        player.BaseAttack += attackIncrease;
        player.Max_Base_HP += hpIncrease;
        player.Base_HP = player.Max_Base_HP;
        
        player.ExperienceToNextLevel = 10 + (player.Level * player.Level * 2);
        
        Console.WriteLine($"\n LEVEL UP! You are now level {player.Level}!");
        Console.WriteLine($"Attack increased by {attackIncrease} (now {player.GetTotalAttack()})");
        Console.WriteLine($"Max HP increased by {hpIncrease} (now {player.GetTotalMaxHp()})");
        Console.WriteLine($"Next level requires {player.ExperienceToNextLevel} experience");
    }
}

