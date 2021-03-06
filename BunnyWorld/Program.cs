﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BunnyWorld
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

		public void TurnToWhite()
		{
			if (sex == "Male")
				Console.WriteLine("Lord {0} of bunny house {1} turned to a White! Kill him and burn his body!", name, house);
			else
				Console.WriteLine("Lady {0} of bunny house {1} turned to a White! Kill her and burn her body!", name, house);
			color = "White";
			house = "White Walker";
		}

		// For debugging purposes.
		public void PrintBunnyData()
		{
			Console.WriteLine("Sex: {0}, Color: {1}, Age: {2}, Name: {3}, House: {4}", sex, color, age, name, house);
		}
	}

	class Program
	{
		enum Colors
		{
			White,
			Brown,
			Grey,
			Black,
			Gold,
			Silver
		};

		enum Houses
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
					color = Enum.GetName(typeof(Colors), new Random().Next(6));
					house = Enum.GetName(typeof(Houses), new Random().Next(4));
				}
				else
				{
					color = "White";
					house = "White Walker";
				}

				Bunny bunny = new Bunny(sex, color, 0, RandomString(10), house); ;
				bunnies.AddLast(bunny);
				PrintANewbornBunny(bunny);
			}

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
				if(bunny.house != "White Walker" && bunny.sex == "Female" && bunny.age >=2)
				{
					Bunny adultMaleBunny = FindAnAdultMale(bunnies);
					if(adultMaleBunny != null)
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
			foreach(Bunny newBornBunny in newbornBunnies)
			{
				bunnies.AddLast(newBornBunny);
				PrintANewbornBunny(newBornBunny);
			}

			// Start the infection of the White Walker bunnies. (Infection includes newborn bunnies).
			WhiteWalkerInfection(bunnies);

			if (bunnies.Count > 1000)
			{
				// Kill half the population randomly.
				LongHardWinter(bunnies);
			}

			// Use when debugging. PrintBunnies(bunnies); 
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

		// Turn Noble bunnies into White Walkers bunnies.
		private static void WhiteWalkerInfection(LinkedList<Bunny> bunnies)
		{
			// We need to exclude all the White Walker bunnies from the Noble bunnies.
			LinkedList<Bunny> nobleBunnies = new LinkedList<Bunny>();
			int whiteWalkersCount = 0;

			foreach (Bunny bunny in bunnies)
			{
				if (bunny.house != "White Walker")
					nobleBunnies.AddLast(bunny);
				else
					whiteWalkersCount++;
			}

			/* Continue for as long as there are more existing Noble bunnies to infect.
			 * Stop when we infect all the needed Noble bunnies, or when we run out of Noble bunnies.
			 */
			while (whiteWalkersCount > 0 && nobleBunnies.Count > 0)
			{
				Bunny randomNobleBunny = GetARandomBunny(nobleBunnies);
				if (randomNobleBunny != null)
				{
					randomNobleBunny.TurnToWhite();
					// Remove this Noble bunny from the list, since now he is a White Walker. (Prevents repetition).
					nobleBunnies.Remove(randomNobleBunny);
				}
				// Downgrading the counter until we infect all the needed Noble bunnies.
				whiteWalkersCount--;
			}
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

		// Print a message for each newborn bunny.
		private static void PrintANewbornBunny(Bunny bunny)
		{
			if(bunny.house != "White Walker")
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
				switch(bunny.house)
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

		// Generate a random string. (For a distinct bunny name).
		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[new Random().Next(s.Length)]).ToArray());
		}

		// Print the entire bunnies list. For debugging purposes.
		private static void PrintBunnies(LinkedList<Bunny> bunnies)
		{
			foreach (Bunny bunny in bunnies)
			{
				bunny.PrintBunnyData();
			}
		}
	}
}
