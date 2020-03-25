using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

/*
========================================================================
BunnyWorld - 
 * A fun game of good Noble houses bunnies and bad White Walker bunnies.
 * The bunnies can breed, have children and die.
 * Play the game and see how the bunny population thrives.
========================================================================
 */

namespace BunnyWorldChallenges
{
	// The bunny object class.
	class Bunny
	{
		public string sex;
		public string color;
		public int age;
		public string name;

		public Bunny(string sex, string color, int age, string name)
		{
			this.sex = sex;
			this.color = color;
			this.age = age;
			this.name = name;
		}

		// Base class functions overridden by each derived class.
		public virtual string HouseName { get; set; }

		public virtual string Sigil { get; set; }

		public virtual string Saying { get; set; }

		public virtual void PrintOnGrid()
		{
			Console.Write("  ");
		}

		// Functions only used by the Targaryen and Dragon classes, declared here for game saving purposes.
		public virtual bool Invincible { get; set; }

		public virtual int Direction { get; set; }
	}

	class Stark : Bunny
	{
		private readonly string houseName = "Stark";
		private readonly string sigil = "Wolf";
		private readonly string saying = "Winter Is Coming!";

		public Stark(string sex, string color, int age, string name) : base( sex,  color,  age, name) { }

		public override string HouseName { get { return houseName; } }

		public override string Sigil { get { return sigil; } }

		public override string Saying { get { return saying; } }

		public override void PrintOnGrid()
		{
			Console.Write((age >= 2) ? "S " : "s ");
		}
	}

	class Baratheon : Bunny
	{
		private readonly string houseName = "Baratheon";
		private readonly string sigil = "Stag";
		private readonly string saying = "Ours Is The Furry!";

		public Baratheon(string sex, string color, int age, string name) : base(sex, color, age, name) { }

		public override string HouseName { get { return houseName; } }

		public override string Sigil { get { return sigil; } }

		public override string Saying { get { return saying; } }

		public override void PrintOnGrid()
		{
			Console.Write((age >= 2) ? "B " : "b ");
		}
	}

	class Lannister : Bunny
	{
		private readonly string houseName = "Lannister";
		private readonly string sigil = "Lion";
		private readonly string saying = "Hear Me Roar!";

		public Lannister(string sex, string color, int age, string name) : base(sex, color, age, name) { }

		public override string HouseName { get { return houseName; } }

		public override string Sigil { get { return sigil; } }

		public override string Saying { get { return saying; } }

		public override void PrintOnGrid()
		{
			Console.Write((age >= 2) ? "L " : "l ");
		}
	}

	class Targaryen : Bunny
	{
		private readonly string houseName = "Targaryen";
		private readonly string sigil = "Dragon";
		private readonly string saying = "Fire And Blood!";

		public Targaryen(string sex, string color, int age, string name) : base(sex, color, age, name) { }

		public override string HouseName { get { return houseName; } }

		public override string Sigil { get { return sigil; } }

		public override string Saying { get { return saying; } }

		public override void PrintOnGrid()
		{
			Console.Write((age >= 2) ? "T " : "t ");
		}

		// A Targaryen affected by the dragon becomes invincible and wins every attack, as attacker or defender.
		public override bool Invincible { get; set; }
	}

	class WhiteWalker : Bunny
	{
		public WhiteWalker(int age, string name) : base("", "White", age, name) { }

		public override string HouseName { get { return "White Walker"; } }

		public override void PrintOnGrid()
		{
			Console.Write("W ");
		}
	}

	// The dragon class. Inherits from bunny.
	class Dragon : Bunny
	{
		public Dragon(string name) : base("", "Red", 0, name) { }

		public override string HouseName { get { return "Dragon"; } }

		public override void PrintOnGrid()
		{
			Console.Write("D ");
		}

		// Track the dragon direction.
		public override int Direction { get; set; }
	}

	// Space object used for representing and finding randomized spaces for new born bunnies and bunnies attacks.
	class Space
	{
		public int x;
		public int y;

		public Space(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	// The main program class.
	class Program
	{
		// Control grid size.
		const int GridSize = 75;

		// Control if the turns run automatically and messages are displayed.
		const bool AutoTurns = true;

		enum Color
		{
			White,
			Brown,
			Grey,
			Black,
			Gold,
			Silver
		};

		private static int dragonX;
		private static int dragonY;

		static void Main()
		{
			LinkedList<Bunny> bunnies = new LinkedList<Bunny>();
			Bunny[,] bunniesGrid = new Bunny[GridSize, GridSize];

			// Place a dragon on the grid. (Using the Bunny class). The dragon will be at least 2 spaces from the edges.
			dragonX = new Random().Next(2, GridSize - 2);
			dragonY = dragonX;

			Dragon dragon = new Dragon(RandomString(10));
			dragon.Direction = 0;
			bunnies.AddLast(dragon);
			bunniesGrid[dragonX, dragonY] = dragon;

			// Inializing the list, creating 8 bunnies. One male and one female for each Noble house.
			for (int i = 0; i < 8; i++)
			{
				string sex;
				if (i % 2 == 0)
					sex = "Male";
				else
					sex = "Female";

				string color = Enum.GetName(typeof(Color), new Random().Next(6));

				Bunny bunny = null;
				switch (i / 2)
				{
					case 0:
						bunny = new Stark(sex, color, 0, RandomString(10));
						break;
					case 1:
						bunny = new Baratheon(sex, color, 0, RandomString(10));
						break;
					case 2:
						bunny = new Lannister(sex, color, 0, RandomString(10));
						break;
					case 3:
						bunny = new Targaryen(sex, color, 0, RandomString(10));
						break;
				}
				
				bunnies.AddLast(bunny);
				PlaceBunnyOnGrid(bunniesGrid, bunny);
				PrintANewbornBunny(bunny);
			}

			PrintBunniesGrid(bunniesGrid);

			NextTurnController(bunnies, bunniesGrid);
		}

		#region Gameplay Handling

		/* Go automatically (control flag in declaration) or listen for click events to move forward to the next turn, save or load game.
		 * Terminate the program when all the bunnies have died.
		 */
		private static void NextTurnController(LinkedList<Bunny> bunnies, Bunny[,] bunniesGrid)
		{
			if (AutoTurns)
			{
				while (!Console.KeyAvailable)
				{
					NextTurn(bunnies, bunniesGrid);
				}
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.S:
						SaveGame(bunnies, bunniesGrid);
						break;
					case ConsoleKey.L:
						LoadSavedGame(bunnies, bunniesGrid);
						break;
				}
			}
			else
			{
				Console.WriteLine("Press any key for the next turn. Press ESC to pause game, then S to save or L to load, Press Esc again to exit.");
				while (Console.ReadKey(true).Key != ConsoleKey.Escape && bunnies.Count > 0)
				{
					if (!Console.KeyAvailable)
					{
						NextTurn(bunnies, bunniesGrid);
					}
				};
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.S:
						SaveGame(bunnies, bunniesGrid);
						break;
					case ConsoleKey.L:
						LoadSavedGame(bunnies, bunniesGrid);
						break;
				}
			}
		}

		// Performing a turn and making modifications.
		private static void NextTurn(LinkedList<Bunny> bunnies, Bunny[,] bunniesGrid)
		{
			LinkedList<Bunny> deadBunnies = new LinkedList<Bunny>();

			// First we age all the bunnies in a unique loop, because we need to age them before we start the breeding.
			foreach (Bunny bunny in bunnies)
			{
				bunny.age++;

				/* A bunny dies when he becomes older than 10 years old.
				 * A White Walker bunny dies when he becomes 50 years old.
				 */
				if (!(bunny is Dragon) && ((!(bunny is WhiteWalker) && bunny.age > 10) || (bunny is WhiteWalker && bunny.age >= 50)))
					deadBunnies.AddLast(bunny);
			}

			// Removing the dead bunnies from the list (needs to happen before breeding, to prevent a dead father).
			foreach (Bunny deadBunny in deadBunnies)
			{
				bunnies.Remove(deadBunny);
				PrintADeadBunny(deadBunny);
			}

			// If the bunny population exceeds 90% of the maximum number of possible bunnies, kill half the population randomly.
			if (bunnies.Count > 0.9 * GridSize * GridSize)
				LongHardWinter(bunnies);

			// Move the dragon in a unique pattern (∞) on the grid.
			MoveDragonOnGrid(bunniesGrid);

			// Move the bunnies one space each turn randomly.
			MoveBunniesOnGrid(bunnies, bunniesGrid);

			/* Go over the entire grid of bunnies and make changes.
			 * We go over the grid and not the linked list, because we need to keep track of mother bunny index.
			 */
			for (int x = 0; x < bunniesGrid.GetLength(0); x++)
			{
				for (int y = 0; y < bunniesGrid.GetLength(1); y++)
				{
					Bunny bunny = bunniesGrid[x, y];

					if (bunny != null)
					{
						// Mating a female bunny with a male bunny.
						if (!(bunny is Dragon) && !(bunny is WhiteWalker) && bunny.sex == "Female" && bunny.age >= 2)
						{
							Bunny adultMaleBunny = FindAnAdultMale(bunnies, bunny.HouseName);
							if (adultMaleBunny != null)
							{
								// Create a new baby bunny.
								CreateABabyBunny(x, y, bunniesGrid, adultMaleBunny, bunnies);
							}
						}
					}
				}
			}

			// We go over the entire grid again, because attacks includes newborn bunnies.
			for (int x = 0; x < bunniesGrid.GetLength(0); x++)
			{
				for (int y = 0; y < bunniesGrid.GetLength(1); y++)
				{
					Bunny bunny = bunniesGrid[x, y];

					if (bunny != null)
					{
						// Start an attack.
						if (!(bunny is Dragon) && !(bunny is WhiteWalker) && bunny.sex == "Male" && bunny.age >= 2)
						{
							AttackBunnies(bunny, x, y, bunniesGrid, bunnies);
						}
					}
				}
			}

			// Kill all bunnies in a certain range.
			DragonAttack(bunnies, bunniesGrid);

			// Print the new grid.
			PrintBunniesGrid(bunniesGrid);
			if(!AutoTurns)
				Console.WriteLine("Press any key for the next turn. Press ESC to pause game, then S to save or L to load, Press Esc again to exit.");
		}

		// Finding a random adult male bunny from a specific Noble house for mating.
		private static Bunny FindAnAdultMale(LinkedList<Bunny> bunnies, string houseName) 
		{
			// Creating a list of all the adult males bunnies.
			LinkedList<Bunny> adultMaleBunnies = new LinkedList<Bunny>();

			foreach (Bunny bunny in bunnies)
			{
				if (bunny.HouseName == houseName && bunny.sex == "Male" && bunny.age >= 2)
				{
					adultMaleBunnies.AddLast(bunny);
				}
			}

			// Return a random adult male bunny.
			return GetARandomBunny(adultMaleBunnies);
		}

		// Get a random bunny from a list of bunnies.
		private static Bunny GetARandomBunny(LinkedList<Bunny> bunnies)
		{
			// Creating a random index to get a randomized bunny.
			int selectedBunnyIndex = new Random().Next(bunnies.Count);
			foreach (Bunny bunny in bunnies)
			{
				if (selectedBunnyIndex == 0)
					return bunny;
				// Downgrading the index until we find the randomized bunny.
				selectedBunnyIndex--;
			}
			return null;
		}

		// Create a new baby bunny.
		private static void CreateABabyBunny(int motherX, int motherY, Bunny[,] bunniesGrid, Bunny fatherBunny, LinkedList<Bunny> bunnies)
		{
			Space emptySpace = GetASpaceFromSurrounding(motherX, motherY, bunniesGrid, true);
			if (emptySpace != null)
			{
				string sex;
				if (new Random().NextDouble() < 0.5)
					sex = "Male";
				else
					sex = "Female";

				Bunny babyBunny = null;

				string color;
				// 2% chance the bunny will be a white walker.
				if (new Random().NextDouble() < 0.98)
				{
					color = bunniesGrid[motherX, motherY].color;
					
					switch (fatherBunny.HouseName)
					{
						case "Stark":
							babyBunny = new Stark(sex, color, 0, RandomString(10));
							break;
						case "Baratheon":
							babyBunny = new Baratheon(sex, color, 0, RandomString(10));
							break;
						case "Lannister":
							babyBunny = new Lannister(sex, color, 0, RandomString(10));
							break;
						case "Targaryen":
							babyBunny = new Targaryen(sex, color, 0, RandomString(10));
							break;
					}
				}
				else
				{
					babyBunny = new WhiteWalker(0, RandomString(10));
				}
		
				bunniesGrid[emptySpace.x, emptySpace.y] = babyBunny;
				bunnies.AddLast(babyBunny);
				PrintANewbornBunny(babyBunny);
			}
		}

		/* Get a random space from surrounding of a given bunny.
		 * Space returned is either empty or occupied depending on the sent flag.
		 * This function is useful for finding an empty space for a newborn bunny, and for finding a surrounding space of a bunny to attack.
		 */
		private static Space GetASpaceFromSurrounding(int bunnyX, int bunnyY, Bunny[,] bunniesGrid, bool shouldBeEmpty)
		{
			// Creating a list of spaces in surrounding of a given bunny.
			LinkedList<Space> spaces = new LinkedList<Space>();

			// Determining the range for searching, to prevent edge cases (out if index).
			int minXRange, maxXRange, minYRange, maxYRange;

			// X axis range.
			if (bunnyX == 0)
			{
				minXRange = 0;
				maxXRange = 2;

			}
			else if (bunnyX == bunniesGrid.GetLength(0) - 1)
			{
				minXRange = bunniesGrid.GetLength(0) - 2;
				maxXRange = bunniesGrid.GetLength(0);
			}
			else
			{
				minXRange = bunnyX - 1;
				maxXRange = bunnyX + 2;
			}

			// Y axis range.
			if (bunnyY == 0)
			{
				minYRange = 0;
				maxYRange = 2;

			}
			else if (bunnyY == bunniesGrid.GetLength(1) - 1)
			{
				minYRange = bunniesGrid.GetLength(1) - 2;
				maxYRange = bunniesGrid.GetLength(1);
			}
			else
			{
				minYRange = bunnyY - 1;
				maxYRange = bunnyY + 2;
			}

			// Start searching for spaces in compliance with range boundaries.
			for (int x = minXRange; x < maxXRange; x++)
				for (int y = minYRange; y < maxYRange; y++)
					if ((shouldBeEmpty && bunniesGrid[x, y] == null) ||
						(!shouldBeEmpty && bunniesGrid[x, y] != null && bunniesGrid[x, y] != bunniesGrid[bunnyX, bunnyY]))
						spaces.AddLast(new Space(x, y));

			// Get a random space from the found spaces and return it.
			if (spaces.Count > 0)
			{
				// Creating a random index to get a randomized space.
				int selectedSpaceIndex = new Random().Next(spaces.Count);
				foreach (Space space in spaces)
				{
					if (selectedSpaceIndex == 0)
						return space;
					// Downgrading the index until we find the randomized space.
					selectedSpaceIndex--;
				}
			}
			return null;
		}

		// Attack bunnies.
		private static void AttackBunnies(Bunny attackerBunny, int attackerX, int attackerY, Bunny[,] bunniesGrid, LinkedList<Bunny> bunnies)
		{
			// Get a space of a surrounding bunny to attack.
			Space victimBunnySpace = GetASpaceFromSurrounding(attackerX, attackerY, bunniesGrid, false);
			if (victimBunnySpace != null)
			{
				Bunny victimBunny = bunniesGrid[victimBunnySpace.x, victimBunnySpace.y];
				// Do something only if they're not from the same house. (Skip if victim is a dragon).
				if (victimBunny.HouseName != attackerBunny.HouseName && !(victimBunny is Dragon))
				{
					// A Targaryen affected by the dragon wins every attack, as attacker or defender.
					if (attackerBunny is Targaryen && ((Targaryen) attackerBunny).Invincible)
					{
						KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, bunniesGrid, bunnies);
					}
					else if(victimBunny is Targaryen && ((Targaryen) victimBunny).Invincible)
					{
						KillBunnyAtIndex(attackerX, attackerY, bunniesGrid, bunnies);
					}
					else if(victimBunny is WhiteWalker)
					{
						// Turn the attacker bunny into a White Walker.
						bunniesGrid[attackerX, attackerY] = null;
						bunnies.Remove(attackerBunny);
						Bunny whiteWalker = new WhiteWalker(attackerBunny.age, attackerBunny.name);
						bunniesGrid[attackerX, attackerY] = whiteWalker;
						bunnies.AddLast(whiteWalker);
						if (!AutoTurns)
						{
							if (attackerBunny.sex == "Male")
								Console.WriteLine("Lord {0} of bunny house {1} turned to a White! Kill him and burn his body!", attackerBunny.name, attackerBunny.HouseName);
							else
								Console.WriteLine("Lady {0} of bunny house {1} turned to a White! Kill her and burn her body!", attackerBunny.name, attackerBunny.HouseName);
						}
					}
					else if(victimBunny.age < 2)
					{
						bunniesGrid[victimBunnySpace.x, victimBunnySpace.y] = null;
						bunnies.Remove(victimBunny);
						PrintADeadBunny(victimBunny);
					}
					// Attack on an adult female from another house.
					else if(victimBunny.sex == "Female")
					{
						// Kill the mother.
						bunniesGrid[victimBunnySpace.x, victimBunnySpace.y] = null;
						bunnies.Remove(victimBunny);
						PrintADeadBunny(victimBunny);

						// Create a new bastard baby bunny in the mother space.
						string sex;
						if (new Random().NextDouble() < 0.5)
							sex = "Male";
						else
							sex = "Female";

						Bunny babyBunny = null;

						string color;
						// 2% chance the bunny will be a white walker.
						if (new Random().NextDouble() < 0.98)
						{
							color = victimBunny.color;

							switch (attackerBunny.HouseName)
							{
								case "Stark":
									babyBunny = new Stark(sex, color, 0, RandomString(10));
									break;
								case "Baratheon":
									babyBunny = new Baratheon(sex, color, 0, RandomString(10));
									break;
								case "Lannister":
									babyBunny = new Lannister(sex, color, 0, RandomString(10));
									break;
								case "Targaryen":
									babyBunny = new Targaryen(sex, color, 0, RandomString(10));
									break;
							}
						}
						else
						{
							babyBunny = new WhiteWalker(0, RandomString(10));
						}

						// Insert the newborn bastard instead of the mother.
						bunniesGrid[victimBunnySpace.x, victimBunnySpace.y] = babyBunny;
						bunnies.AddLast(babyBunny);
						PrintANewbornBunny(babyBunny);
					}
					else
					switch (attackerBunny.HouseName)
					{
						case "Stark":
								if(victimBunny is Baratheon || victimBunny is Lannister)
								{
									KillBunnyAtIndex(attackerX, attackerY, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, bunniesGrid, bunnies);
								}
								break;
						case "Baratheon":
								if (victimBunny is Stark || victimBunny is Targaryen)
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(attackerX, attackerY, bunniesGrid, bunnies);
								}
								break;
						case "Lannister":
								if (victimBunny is Stark || victimBunny is Baratheon)
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(attackerX, attackerY, bunniesGrid, bunnies);
								}
								break;
						case "Targaryen":
								if (victimBunny is Stark || victimBunny is Baratheon)
								{
									KillBunnyAtIndex(attackerX, attackerY, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, bunniesGrid, bunnies);
								}
								break;
					}
				}
			}
		}

		// Kill a bunny at a given index.
		private static void KillBunnyAtIndex(int x, int y, Bunny[,] bunniesGrid, LinkedList<Bunny> bunnies)
		{
			Bunny bunny = bunniesGrid[x, y];
			bunniesGrid[x, y] = null;
			bunnies.Remove(bunny);
			PrintADeadBunny(bunny);
		}

		// Kill exactly half the bunnies population randomly.
		private static void LongHardWinter(LinkedList<Bunny> bunnies)
		{
			// Marking the half population count.
			int halfBunniesPopulation = bunnies.Count / 2;

			// Kill bunnies until we reach the half population count.
			while (bunnies.Count > halfBunniesPopulation)
			{
				Bunny randomBunny = GetARandomBunny(bunnies);
				if (randomBunny != null && !(randomBunny is Dragon))
				{
					bunnies.Remove(randomBunny);
					PrintADeadBunny(randomBunny);
				}
			}
		}

		// Print a message for each newborn bunny.
		private static void PrintANewbornBunny(Bunny bunny)
		{
			if (!AutoTurns)
			{
				if (!(bunny is WhiteWalker))
				{
					if (bunny.sex == "Male")
						Console.WriteLine("Lord {0} of bunny house {1} of color {2} was born!", bunny.name, bunny.HouseName, bunny.color);
					else
						Console.WriteLine("Lady {0} of bunny house {1} of color {2} was born!", bunny.name, bunny.HouseName, bunny.color);
				}
				else
					Console.WriteLine("White Walker bunny {0} was born!", bunny.name);
			}
		}

		// Print a message for each dead bunny.
		private static void PrintADeadBunny(Bunny bunny)
		{
			if (!AutoTurns)
			{
				if (!(bunny is WhiteWalker))
				{
					if (bunny.sex == "Male")
						Console.WriteLine("Lord {0} of bunny house {1} died at age {2}! He was a fierce {3}! {4}", bunny.name, bunny.HouseName, bunny.age, bunny.Sigil, bunny.Saying);
					else
						Console.WriteLine("Lady {0} of bunny house {1} died at age {2}! She was a fierce {3}! {4}", bunny.name, bunny.HouseName, bunny.age, bunny.Sigil, bunny.Saying);
				}
				else
					Console.WriteLine("White Walker {0} died at age {1}!", bunny.name, bunny.age);
			}
		}

		#endregion

		#region Bunnies Grid Handling

		// Place a bunny on the grid in a random empty space.
		private static void PlaceBunnyOnGrid(Bunny[,] bunniesGrid, Bunny bunny)
		{
			int radomBunnyX = new Random().Next(GridSize);
			int radomBunnyY = new Random().Next(GridSize);

			while (bunniesGrid[radomBunnyX, radomBunnyY] != null)
			{
				radomBunnyX = new Random().Next(GridSize);
				radomBunnyY = new Random().Next(GridSize);
			}

			bunniesGrid[radomBunnyX, radomBunnyY] = bunny;
		}

		// Move the dragon in a unique pattern (∞) on the grid.
		private static void MoveDragonOnGrid(Bunny[,] bunniesGrid)
		{
			Dragon dragon = (Dragon) bunniesGrid[dragonX, dragonY];
			Array.Clear(bunniesGrid, 0, bunniesGrid.Length);

			switch (dragonX)
			{
				case 2 when dragonY == 2:
					dragon.Direction = 0;
					break;
				case GridSize - 3 when dragonY == GridSize - 3:
					dragon.Direction = 1;
					break;
				case 2 when dragonY == GridSize - 3:
					dragon.Direction = 2;
					break;
				case GridSize - 3 when dragonY == 2:
					dragon.Direction = 3;
					break;
			}


			switch (dragon.Direction)
			{
				case 0:
					dragonX++;
					dragonY++;
					break;
				case 1:
					dragonX--;
					break;
				case 2:
					dragonX++;
					dragonY--;
					break;
				case 3:
					dragonX--;
					break;
			}

			bunniesGrid[dragonX, dragonY] = dragon;
		}

		// Move the bunnies on the grid to a new random space.
		private static void MoveBunniesOnGrid(LinkedList<Bunny> bunnies, Bunny[,] bunniesGrid)
		{
			foreach (Bunny bunny in bunnies)
			{
				// Check if there is enough space on the grid before we place another bunny.
				if (!(bunny is Dragon) && bunnies.Count <= GridSize * GridSize)
					PlaceBunnyOnGrid(bunniesGrid, bunny);
			}

			// In case the number of bunnies is greater than the number of cells in the grid.
			if (bunnies.Count > GridSize * GridSize)
				Console.WriteLine("Not enough space on the grid!");
		}

		// Kill all bunnies in a certain range.
		private static void DragonAttack(LinkedList<Bunny> bunnies, Bunny[,] bunniesGrid)
		{
			for (int x = dragonX - 2; x < dragonX + 3; x++)
			{
				for (int y = dragonY - 2; y < dragonY + 3; y++)
				{
					if(bunniesGrid[x, y] != null && !(bunniesGrid[x, y] is Dragon))
					{
						if(bunniesGrid[x, y] is Targaryen)
						{
							((Targaryen) bunniesGrid[x, y]).Invincible = true;
						}
						else
						{
							KillBunnyAtIndex(x, y, bunniesGrid, bunnies);
						}
					}
				}
			}
		}

		// Print the grid of bunnies.
		private static void PrintBunniesGrid(Bunny[,] bunniesGrid)
		{
			if (AutoTurns)
				Console.Clear();
			for (int x = 0; x < bunniesGrid.GetLength(0); x++)
			{
				for (int y = 0; y < bunniesGrid.GetLength(1); y++)
				{
					if (bunniesGrid[x, y] != null)
					{
						ChangeConsoleColor(bunniesGrid[x, y].color);
						bunniesGrid[x, y].PrintOnGrid();
						Console.ResetColor();
					}
					else
					{
						Console.Write("  ");
					}
				}
				Console.WriteLine();
			}
		}

		#endregion

		// Change the console output color.
		private static void ChangeConsoleColor(string color)
		{
			Console.ForegroundColor = color switch
			{
				"White" => ConsoleColor.White,
				"Brown" => ConsoleColor.Blue,
				"Grey" => ConsoleColor.Green,
				"Black" => ConsoleColor.DarkGray,
				"Gold" => ConsoleColor.Yellow,
				"Silver" => ConsoleColor.Cyan,
				"Red" => ConsoleColor.Red,
				_ => ConsoleColor.White,
			};
		}

		// Generate a random string. (For a distinct bunny name).
		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[new Random().Next(s.Length)]).ToArray());
		}

        #region Game Saving Handaling

		// Save current game state into a json file.
		public static void SaveGame(LinkedList<Bunny> bunnies, Bunny[,] bunniesGrid)
		{
			// Open file stream.
			using (StreamWriter file = File.CreateText(@"C:\Users\itiel\Desktop\BunnyWorldSavedGame.json"))
			{
				JsonSerializer serializer = new JsonSerializer();
				// Serialize the data directly into file stream.
				serializer.Serialize(file, bunniesGrid);
			}

			Console.WriteLine("Game saved to file (C:/Users/itiel/Desktop/BunnyWorldSavedGame.json).");

			if (AutoTurns)
			{
				Console.WriteLine("Press any key to continue the game. Press ESC to exit.");
				if (Console.ReadKey(true).Key != ConsoleKey.Escape)
				{
					NextTurnController(bunnies, bunniesGrid);
				}
			}
			else
				NextTurnController(bunnies, bunniesGrid);
		}

		// Load a saved game state from a json file.
		public static void LoadSavedGame(LinkedList<Bunny> bunnies, Bunny[, ] bunniesGrid)
		{
			// Clear current game data.
			bunnies.Clear();
			Array.Clear(bunniesGrid, 0, bunniesGrid.Length);

			// Deserialize the data from the json file.
			string json = File.ReadAllText(@"C:\Users\itiel\Desktop\BunnyWorldSavedGame.json");

			Bunny[,] savedBunniesGrid = JsonConvert.DeserializeObject<Bunny[,]>(json);

			for (int x = 0; x < savedBunniesGrid.GetLength(0); x++)
			{
				for (int y = 0; y < savedBunniesGrid.GetLength(1); y++)
				{
					if (savedBunniesGrid[x, y] != null)
					{
						Bunny bunny = null;
						switch (savedBunniesGrid[x, y].HouseName)
						{
							case "Stark":
								bunny = new Stark(savedBunniesGrid[x, y].sex, savedBunniesGrid[x, y].color, savedBunniesGrid[x, y].age, savedBunniesGrid[x, y].name);
								break;
							case "Baratheon":
								bunny = new Baratheon(savedBunniesGrid[x, y].sex, savedBunniesGrid[x, y].color, savedBunniesGrid[x, y].age, savedBunniesGrid[x, y].name);
								break;
							case "Lannister":
								bunny = new Lannister(savedBunniesGrid[x, y].sex, savedBunniesGrid[x, y].color, savedBunniesGrid[x, y].age, savedBunniesGrid[x, y].name);
								break;
							case "Targaryen":
								bunny = new Targaryen(savedBunniesGrid[x, y].sex, savedBunniesGrid[x, y].color, savedBunniesGrid[x, y].age, savedBunniesGrid[x, y].name);
								((Targaryen)bunny).Invincible = savedBunniesGrid[x, y].Invincible;
								break;
							case "White Walker":
								bunny = new WhiteWalker(savedBunniesGrid[x, y].age, savedBunniesGrid[x, y].name);
								break;
							case "Dragon":
								bunny = new Dragon(savedBunniesGrid[x, y].name);
								dragonX = x;
								dragonY = y;
								((Dragon)bunny).Direction = savedBunniesGrid[x, y].Direction;
								break;
						}
						bunnies.AddLast(bunny);
						bunniesGrid[x, y] = bunny;
					}
				}
			}

			PrintBunniesGrid(bunniesGrid);

			NextTurnController(bunnies, bunniesGrid);
		}

		#endregion
	}
}