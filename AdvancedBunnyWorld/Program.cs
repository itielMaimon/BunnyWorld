﻿using System;
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

namespace AdvancedBunnyWorld
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
		public void TurnToWhite()
		{
			if (sex == "Male")
				Console.WriteLine("Lord {0} of bunny house {1} turned to a White! Kill him and burn his body!", name, house);
			else
				Console.WriteLine("Lady {0} of bunny house {1} turned to a White! Kill her and burn her body!", name, house);
			color = "White";
			house = "White Walker";
		}
	}

	// The main program class.
	class Program
	{
		const int GridSize = 75;

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
			Console.WriteLine("Here are the bunnies:");

			LinkedList<Bunny> bunnies = new LinkedList<Bunny>();
			Bunny[,] bunniesGrid = new Bunny[GridSize, GridSize];

			// Inializing the list, creating 5 bunnies.
			for (int i = 0; i < 5; i++)
			{
				string sex;
				if (new Random().NextDouble() < 0.5)
					sex = "Male";
				else
					sex = "Female";

				string color;
				string house;
				/// 2% chance the bunny will be a white walker.
				if (new Random().NextDouble() < 0.98)
				{
					color = Enum.GetName(typeof(Color), new Random().Next(6));
					house = Enum.GetName(typeof(House), new Random().Next(4));
				}
				else
				{
					color = "White";
					house = "White Walker";
				}

				Bunny bunny = new Bunny(sex, color, 0, RandomString(10), house); ;
				bunnies.AddLast(bunny);
				PlaceBunnyOnGrid(bunniesGrid, bunny);
				PrintANewbornBunny(bunny);
			}

			PrintBunniesGrid(bunniesGrid);

			/* Listen for click events to move forward to the next turn.
			 * Terminate the program when all the bunnies have died.
			 */
			Console.WriteLine("Press any key for the next turn. Press ESC to stop.");
			while (Console.ReadKey(true).Key != ConsoleKey.Escape && bunnies.Count > 0)
			{
				if (!Console.KeyAvailable)
				{
					NextTurn(bunnies);
				}
			};
		}

		#region Gameplay Handling

		// Performing a turn and making modifications.
		private static void NextTurn(LinkedList<Bunny> bunnies)
		{
			LinkedList<Bunny> deadBunnies = new LinkedList<Bunny>();
			LinkedList<Bunny> newbornBunnies = new LinkedList<Bunny>();

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

			// Creating another foreach loop, because we need to age all the bunnies before we start the breeding.
			foreach (Bunny bunny in bunnies)
			{
				// Mating a female bunny with a male bunny.
				if (bunny.house != "White Walker" && bunny.sex == "Female" && bunny.age >= 2)
				{
					Bunny adultMaleBunny = FindAnAdultMale(bunnies);
					if (adultMaleBunny != null)
					{
						// Create a new baby bunny.
						string sex;
						if (new Random().NextDouble() < 0.5)
							sex = "Male";
						else
							sex = "Female";

						string color;
						string house;
						/// 2% chance the bunny will be a white walker.
						if (new Random().NextDouble() < 0.98)
						{
							color = bunny.color;
							house = adultMaleBunny.house;
						}
						else
						{
							color = "White";
							house = "White Walker";
						}

						newbornBunnies.AddLast(new Bunny(sex, color, 0, RandomString(10), house));
					}
				}
			}

			// Adding the newborn bunnies to the list.
			foreach (Bunny newBornBunny in newbornBunnies)
			{
				bunnies.AddLast(newBornBunny);
				PrintANewbornBunny(newBornBunny);
			}

			if (bunnies.Count > 1000)
			{
				// Kill half the population randomly.
				LongHardWinter(bunnies);
			}

			// Move the bunnies one space each turn randomly.
			Bunny[,] bunniesGrid = MoveBunniesOnGrid(bunnies);

			/* Start the infection of the White Walker bunnies. (Infection includes newborn bunnies).
			 * Infection starts only after the bunnies moved to their new space.
			 * Infection does not include new infected bunnies that turned into White Walkers bunnies this turn,
			 * i.e. a Noble bunny that got infected and turned into a White Walker bunny this turn, will only infect bunnies at the next turn.
			 */
			WhiteWalkerInfection(bunniesGrid);

			// Print the new grid.
			PrintBunniesGrid(bunniesGrid);
			Console.WriteLine("Press any key for next turn. Press ESC to stop.");
		}

		// Finding a random adult male bunny for mating.
		private static Bunny FindAnAdultMale(LinkedList<Bunny> bunnies)
		{
			// Creating a list of all the adult males bunnies.
			LinkedList<Bunny> adultMaleBunnies = new LinkedList<Bunny>();

			foreach (Bunny bunny in bunnies)
			{
				if (bunny.house != "White Walker" && bunny.sex == "Male" && bunny.age >= 2)
					adultMaleBunnies.AddLast(bunny);
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

		// Turn Noble bunnies into White Walkers bunnies.
		private static void WhiteWalkerInfection(Bunny[,] bunniesGrid)
		{
			/* We first creating a list of selected bunnies and only after we go over the entire grid we turn them into White Walker bunnies.
			 * This is done in order to prevent a new infected bunny that turned into a White Walker bunny to infect more bunnies in its surronding.
			 * New White Walker bunnies will only infect other bunnies at the next turn.
			 */
			LinkedList<Bunny> infectedNobleBunnies = new LinkedList<Bunny>();

			// Go over the entire grid of bunnies to find the White Walker bunnies.
			for (int x = 0; x < bunniesGrid.GetLength(0); x++)
			{
				for (int y = 0; y < bunniesGrid.GetLength(1); y++)
				{
					if (bunniesGrid[x, y] != null && bunniesGrid[x, y].house == "White Walker")
					{
						// Search for a Noble bunny in its surrounding and add him into the infected bunnies list.
						Bunny randomNobleBunnyFromSurrounding = GetANobleBunnyFromSurrounding(x, y, bunniesGrid, infectedNobleBunnies);
						if (randomNobleBunnyFromSurrounding != null)
							infectedNobleBunnies.AddLast(randomNobleBunnyFromSurrounding);
					}
				}
			}

			// Turn the infected bunnies into White Walker bunnies only after we went over the entire grid.
			foreach(Bunny infectedBunny in infectedNobleBunnies)
			{
				infectedBunny.TurnToWhite();
			}
		}

		// Get a random Noble bunny from surrounding of a White Walker bunny.
		private static Bunny GetANobleBunnyFromSurrounding(int whiteX, int whiteY, Bunny[,] bunniesGrid, LinkedList<Bunny> infectedNobleBunnies)
		{
			// Creating a list of Noble bunnies in surrounding of a White Walker bunny.
			LinkedList<Bunny> nobleBunnies = new LinkedList<Bunny>();

			// Determining the range for searching, to prevent edge cases (out if index).
			int minXRange, maxXRange, minYRange, maxYRange;

			// X axis range.
			if (whiteX == 0)
			{
				minXRange = 0;
				maxXRange = 2;

			}
			else if (whiteX == bunniesGrid.GetLength(0) - 1)
			{
				minXRange = bunniesGrid.GetLength(0) - 2;
				maxXRange = bunniesGrid.GetLength(0);
			}
			else
			{
				minXRange = whiteX - 1;
				maxXRange = whiteX + 2;
			}

			// Y axis range.
			if (whiteY == 0)
			{
				minYRange = 0;
				maxYRange = 2;

			}
			else if (whiteY == bunniesGrid.GetLength(1) - 1)
			{
				minYRange = bunniesGrid.GetLength(1) - 2;
				maxYRange = bunniesGrid.GetLength(1);
			}
			else
			{
				minYRange = whiteY - 1;
				maxYRange = whiteY + 2;
			}

			// Start searching in compliance with range boundaries.
			for (int x = minXRange; x < maxXRange; x++)
				for (int y = minYRange; y < maxYRange; y++)
				{
					/* Check to prevent empty spaces, White Walker bunnies and already chosen Noble bunnies to be infected.
					 * (A bunny can be selected by only one White Walker bunny).
					 */
					if (bunniesGrid[x, y] != null && bunniesGrid[x, y].house != "White Walker" && !infectedNobleBunnies.Contains(bunniesGrid[x, y]))
						nobleBunnies.AddLast(bunniesGrid[x, y]);
				}

			// Get a random Noble bunny from the found Noble bunnies and return it.
			if (nobleBunnies.Count > 0)
				return GetARandomBunny(nobleBunnies);
			return null;
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

		// Print a message for each dead bunny.
		private static void PrintADeadBunny(Bunny bunny)
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
			for (int x = 0; x < bunniesGrid.GetLength(0); x++)
			{
				for (int y = 0; y < bunniesGrid.GetLength(1); y++)
				{
					if (bunniesGrid[x, y] != null)
					{
						switch (bunniesGrid[x, y].house)
						{
							case "Stark":
								Console.ForegroundColor = ConsoleColor.Red;
								Console.Write((bunniesGrid[x, y].age >= 2) ? "S " : "s ");
								Console.ResetColor();
								break;
							case "Baratheon":
								Console.ForegroundColor = ConsoleColor.Green;
								Console.Write((bunniesGrid[x, y].age >= 2) ? "B " : "b ");
								Console.ResetColor();
								break;
							case "Lannister":
								Console.ForegroundColor = ConsoleColor.Yellow;
								Console.Write((bunniesGrid[x, y].age >= 2) ? "L " : "l ");
								Console.ResetColor();
								break;
							case "Targaryen":
								Console.ForegroundColor = ConsoleColor.Cyan;
								Console.Write((bunniesGrid[x, y].age >= 2) ? "T " : "t ");
								Console.ResetColor();
								break;
							case "White Walker":
								Console.ForegroundColor = ConsoleColor.White;
								Console.Write("W ");
								Console.ResetColor();
								break;
						}
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
        
		// Generate a random string. (For a distinct bunny name).
		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[new Random().Next(s.Length)]).ToArray());
		}
	}
}