using System;
using System.Collections.Generic;
using System.Linq;

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
		public string house;

		public Bunny(string sex, string color, int age, string name, string house)
		{
			this.sex = sex;
			this.color = color;
			this.age = age;
			this.name = name;
			this.house = house;
		}

		// Turn a Noble bunny into a White Walker.
		public void TurnToWhite(bool autoTurns)
		{
			if (!autoTurns)
			{
				if (sex == "Male")
					Console.WriteLine("Lord {0} of bunny house {1} turned to a White! Kill him and burn his body!", name, house);
				else
					Console.WriteLine("Lady {0} of bunny house {1} turned to a White! Kill her and burn her body!", name, house);
			}
			color = "White";
			house = "White Walker";
		}
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

		enum House
		{
			Stark,
			Baratheon,
			Lannister,
			Targaryen
		};

		static void Main(string[] args)
		{
			LinkedList<Bunny> bunnies = new LinkedList<Bunny>();
			Bunny[,] bunniesGrid = new Bunny[GridSize, GridSize];

			// Inializing the list, creating 8 bunnies. One male and one female for each Noble house.
			for (int i = 0; i < 8; i++)
			{
				string sex;
				if (i % 2 == 0)
					sex = "Male";
				else
					sex = "Female";

				string color = Enum.GetName(typeof(Color), new Random().Next(6));
				string house = Enum.GetName(typeof(House), i / 2);

				Bunny bunny = new Bunny(sex, color, 0, RandomString(10), house);
				bunnies.AddLast(bunny);
				PlaceBunnyOnGrid(bunniesGrid, bunny);
				PrintANewbornBunny(bunny);
			}

			PrintBunniesGrid(bunniesGrid);

			/* Go automatically or listen for click events to move forward to the next turn. (Control flag in declaration).
			 * Terminate the program when all the bunnies have died.
			 */
			if (AutoTurns)
			{
				while (bunnies.Count > 0)
				{
					NextTurn(bunnies);
				};
			}
			else
			{
				Console.WriteLine("Press any key for the next turn. Press ESC to stop.");
				while (Console.ReadKey(true).Key != ConsoleKey.Escape && bunnies.Count > 0)
				{
					if (!Console.KeyAvailable)
					{
						NextTurn(bunnies);
					}
				};
			}
		}

		#region Gameplay Handling

		// Performing a turn and making modifications.
		private static void NextTurn(LinkedList<Bunny> bunnies)
		{
			LinkedList<Bunny> deadBunnies = new LinkedList<Bunny>();

			// First we age all the bunnies in a unique loop, because we need to age them before we start the breeding.
			foreach (Bunny bunny in bunnies)
			{
				bunny.age++;

				/* A bunny dies when he becomes older than 10 years old.
				 * A White Walker bunny dies when he becomes 50 years old.
				 */
				if ((bunny.house != "White Walker" && bunny.age > 10) || (bunny.house == "White Walker" && bunny.age >= 50))
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

			// Move the bunnies one space each turn randomly.
			Bunny[,] bunniesGrid = MoveBunniesOnGrid(bunnies);

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
						if (bunny.house != "White Walker" && bunny.sex == "Female" && bunny.age >= 2)
						{
							Bunny adultMaleBunny = FindAnAdultMale(bunnies, bunny.house);
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
						if (bunny.house != "White Walker" && bunny.sex == "Male" && bunny.age >= 2)
						{
							AttackBunnies(bunny, x, y, bunniesGrid, bunnies);
						}
					}
				}
			}

			// Print the new grid.
			PrintBunniesGrid(bunniesGrid);
			if(!AutoTurns)
				Console.WriteLine("Press any key for next turn. Press ESC to stop.");
		}

		// Finding a random adult male bunny from a specific Noble house for mating.
		private static Bunny FindAnAdultMale(LinkedList<Bunny> bunnies, string house)
		{
			// Creating a list of all the adult males bunnies.
			LinkedList<Bunny> adultMaleBunnies = new LinkedList<Bunny>();

			foreach (Bunny bunny in bunnies)
			{
				if (bunny.house == house && bunny.sex == "Male" && bunny.age >= 2)
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

				string color;
				string house;
				// 2% chance the bunny will be a white walker.
				if (new Random().NextDouble() < 0.98)
				{
					color = bunniesGrid[motherX, motherY].color;
					house = fatherBunny.house;
				}
				else
				{
					color = "White";
					house = "White Walker";
				}
				Bunny babyBunny = new Bunny(sex, color, 0, RandomString(10), house);
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
			if(victimBunnySpace != null)
			{
				Bunny victimBunny = bunniesGrid[victimBunnySpace.x, victimBunnySpace.y];
				// Do something only if they're not from the same house.
				if (victimBunny.house != attackerBunny.house)
				{
					if(victimBunny.house == "White Walker")
					{
						attackerBunny.TurnToWhite(AutoTurns);
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

						string color;
						string house;
						// 2% chance the bunny will be a white walker.
						if (new Random().NextDouble() < 0.98)
						{
							color = victimBunny.color;
							house = attackerBunny.house;
						}
						else
						{
							color = "White";
							house = "White Walker";
						}
						// Insert the newborn bastard instead of the mother.
						Bunny babyBunny = new Bunny(sex, color, 0, RandomString(10), house);
						bunniesGrid[victimBunnySpace.x, victimBunnySpace.y] = babyBunny;
						bunnies.AddLast(babyBunny);
						PrintANewbornBunny(babyBunny);
					}
					else
					switch (attackerBunny.house)
					{
						case "Stark":
								if(victimBunny.house == "Baratheon" || victimBunny.house == "Lannister")
								{
									KillBunnyAtIndex(attackerX, attackerY, attackerBunny, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, victimBunny, bunniesGrid, bunnies);
								}
								break;
						case "Baratheon":
								if (victimBunny.house == "Stark" || victimBunny.house == "Targaryen")
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, victimBunny, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(attackerX, attackerY, attackerBunny, bunniesGrid, bunnies);
								}
								break;
						case "Lannister":
								if (victimBunny.house == "Stark" || victimBunny.house == "Baratheon")
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, victimBunny, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(attackerX, attackerY, attackerBunny, bunniesGrid, bunnies);
								}
								break;
						case "Targaryen":
								if (victimBunny.house == "Stark" || victimBunny.house == "Baratheon")
								{
									KillBunnyAtIndex(attackerX, attackerY, attackerBunny, bunniesGrid, bunnies);
								}
								else
								{
									KillBunnyAtIndex(victimBunnySpace.x, victimBunnySpace.y, victimBunny, bunniesGrid, bunnies);
								}
								break;
					}
				}
			}
		}

		// Kill a bunny at a given index.
		private static void KillBunnyAtIndex(int x, int y, Bunny bunny, Bunny[,] bunniesGrid, LinkedList<Bunny> bunnies)
		{
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
				if (randomBunny != null)
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
				if (bunny.house != "White Walker")
				{
					if (bunny.sex == "Male")
						Console.WriteLine("Lord {0} of bunny house {1} of color {2} was born!", bunny.name, bunny.house, bunny.color);
					else
						Console.WriteLine("Lady {0} of bunny house {1} of color {2} was born!", bunny.name, bunny.house, bunny.color);
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
				if (bunny.house != "White Walker")
				{
					string sigil;
					string saying;
					switch (bunny.house)
					{
						case "Stark":
							sigil = "Wolf";
							saying = "Winter Is Coming!";
							break;
						case "Baratheon":
							sigil = "Stag";
							saying = "Ours Is The Furry!";
							break;
						case "Lannister":
							sigil = "Lion";
							saying = "Hear Me Roar!";
							break;
						case "Targaryen":
							sigil = "Dragon";
							saying = "Fire And Blood!";
							break;
						default:
							sigil = "";
							saying = "";
							break;
					}
					if (bunny.sex == "Male")
						Console.WriteLine("Lord {0} of bunny house {1} died at age {2}! He was a fierce {3}! {4}", bunny.name, bunny.house, bunny.age, sigil, saying);
					else
						Console.WriteLine("Lady {0} of bunny house {1} died at age {2}! She was a fierce {3}! {4}", bunny.name, bunny.house, bunny.age, sigil, saying);
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

		// Move the bunnies on the grid to a new random space.
		private static Bunny[,] MoveBunniesOnGrid(LinkedList<Bunny> bunnies)
		{
			Bunny[,] bunniesGrid = new Bunny[GridSize, GridSize];
			foreach (Bunny bunny in bunnies)
			{
				// Check if there is enough space on the grid before we place another bunny.
				if (bunnies.Count <= GridSize * GridSize)
					PlaceBunnyOnGrid(bunniesGrid, bunny);
			}

			// In case the number of bunnies is greater than the number of cells in the grid.
			if (bunnies.Count > GridSize * GridSize)
				Console.WriteLine("Not enough space on the grid!");

			return bunniesGrid;
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
						switch (bunniesGrid[x, y].house)
						{
							case "Stark":
								Console.Write((bunniesGrid[x, y].age >= 2) ? "S " : "s ");
								break;
							case "Baratheon":
								Console.Write((bunniesGrid[x, y].age >= 2) ? "B " : "b ");
								break;
							case "Lannister":
								Console.Write((bunniesGrid[x, y].age >= 2) ? "L " : "l ");
								break;
							case "Targaryen":
								Console.Write((bunniesGrid[x, y].age >= 2) ? "T " : "t ");
								break;
							case "White Walker":
								Console.Write("W ");
								break;
						}
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
				"Brown" => ConsoleColor.Red,
				"Grey" => ConsoleColor.Magenta,
				"Black" => ConsoleColor.DarkGray,
				"Gold" => ConsoleColor.Yellow,
				"Silver" => ConsoleColor.Cyan,
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
	}
}