using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace zad_9
{
    class Message
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Tworzy nową wiadomość z nadawcą i treścią.
        /// </summary>
        public Message(string sender, string content)
        {
            Sender = sender;
            Content = content;
        }
        /// <summary>
        /// Zwraca tekstową reprezentację wiadomości.
        /// </summary>
        public override string ToString()
        {
            return $"[{Sender}] {Content}";
        }
    }

    class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public List<Message> Inbox { get; private set; }
        /// <summary>
        /// Tworzy nowego użytkownika i inicjalizuje skrzynkę odbiorczą.
        /// </summary>
        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Inbox = new List<Message>();
        }
    }

    class Messenger
    {
        private Dictionary<string, User> users;

        private User? loggedUser;

        private const string FILE_NAME = "users.txt";
        /// <summary>
        /// Tworzy obiekt komunikatora i wczytuje użytkowników z pliku.
        /// </summary>
        public Messenger()
        {
            users = new Dictionary<string, User>();

            LoadUsersFromFile();
        }
        public string LoggedUsername
        {
            get
            {
                if (loggedUser == null)
                    return "Brak";

                return loggedUser.Username;
            }
        }
        /// <summary>
        /// Rejestruje nowego użytkownika w systemie.
        /// </summary>
        public void Register(string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new EmptyUsernameException();

            if (string.IsNullOrWhiteSpace(password))
                throw new EmptyPasswordException();

            if (password != confirmPassword)
                throw new PasswordMismatchException();

            if (users.ContainsKey(username))
                throw new UserAlreadyExistsException();

            User user = new User(username, password);

            users.Add(username, user);

            SaveUserToFile(user);
        }
        /// <summary>
        /// Loguje użytkownika do systemu.
        /// </summary>
        public void Login(string username, string password)
        {
            if (loggedUser != null)
                throw new UserAlreadyLoggedException();

            if (!users.ContainsKey(username))
                throw new UserNotFoundException();

            User user = users[username];

            if (user.Password != password)
                throw new InvalidPasswordException();

            loggedUser = user;
        }
        /// <summary>
        /// Wylogowuje aktualnie zalogowanego użytkownika.
        /// </summary>
        public void Logout()
        {
            if (loggedUser == null)
                throw new NoLoggedUserException();

            loggedUser = null;
        }
        /// <summary>
        /// Wysyła wiadomość do wskazanego użytkownika.
        /// </summary>
        public void SendMessage(string receiverUsername, string content)
        {
            if (loggedUser == null)
                throw new NoLoggedUserException();

            if (!users.ContainsKey(receiverUsername))
                throw new UserNotFoundException();

            if (string.IsNullOrWhiteSpace(content))
                throw new EmptyMessageException();

            Message msg = new Message(loggedUser.Username, content);

            users[receiverUsername].Inbox.Add(msg);
        }
        /// <summary>
        /// Zwraca listę nadawców wraz z liczbą otrzymanych wiadomości.
        /// </summary>
        public Dictionary<string, int> GetSenders()
        {
            if (loggedUser == null)
                throw new InvalidOperationException("Musisz być zalogowany.");

            return loggedUser.Inbox
                .GroupBy(m => m.Sender)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        /// <summary>
        /// Pobiera wszystkie wiadomości od wybranego nadawcy.
        /// </summary>
        public List<Message> GetMessagesFromSender(string sender)
        {
            if (loggedUser == null)
                throw new NoLoggedUserException();

            List<Message> messages = loggedUser.Inbox
                .Where(m => m.Sender == sender)
                .ToList();

            if (messages.Count == 0)
                throw new NoMessagesException();

            return messages;
        }
        /// <summary>
        /// Zapisuje dane użytkownika do pliku.
        /// </summary>
        private void SaveUserToFile(User user)
        {
            File.AppendAllText(
                FILE_NAME,
                $"{user.Username};{user.Password}{Environment.NewLine}"
            );
        }
        /// <summary>
        /// Wczytuje użytkowników zapisanych w pliku.
        /// </summary>
        private void LoadUsersFromFile()
        {
            if (!File.Exists(FILE_NAME))
                return;

            string[] lines = File.ReadAllLines(FILE_NAME);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split(';');

                if (parts.Length != 2)
                    continue;

                string username = parts[0];
                string password = parts[1];

                users[username] = new User(username, password);
            }
        }
    }

    class EmptyUsernameException : Exception
    {
        public EmptyUsernameException()
            : base("Nazwa użytkownika nie może być pusta.")
        {
        }
    }

    class EmptyPasswordException : Exception
    {
        public EmptyPasswordException()
            : base("Hasło nie może być puste.")
        {
        }
    }

    class PasswordMismatchException : Exception
    {
        public PasswordMismatchException()
            : base("Hasła nie są identyczne.")
        {
        }
    }

    class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException()
            : base("Użytkownik już istnieje.")
        {
        }
    }

    class UserNotFoundException : Exception
    {
        public UserNotFoundException()
            : base("Użytkownik nie istnieje.")
        {
        }
    }

    class UserAlreadyLoggedException : Exception
    {
        public UserAlreadyLoggedException()
            : base("Użytkownik jest już zalogowany.")
        {
        }
    }

    class NoLoggedUserException : Exception
    {
        public NoLoggedUserException()
            : base("Brak zalogowanego użytkownika.")
        {
        }
    }

    class InvalidPasswordException : Exception
    {
        public InvalidPasswordException()
            : base("Niepoprawne hasło.")
        {
        }
    }

    class EmptyMessageException : Exception
    {
        public EmptyMessageException()
            : base("Treść wiadomości nie może być pusta.")
        {
        }
    }

    class NoMessagesException : Exception
    {
        public NoMessagesException()
            : base("Brak wiadomości od tego użytkownika.")
        {
        }
    }

    class Program
    {
        /// <summary>
        /// Główna metoda programu obsługująca menu komunikatora.
        /// </summary>
        static void Main(string[] args)
        {
            Messenger messenger = new Messenger();

            bool running = true;

            while (running)
            {
                Console.Clear();

                Console.WriteLine("=== KOMUNIKATOR ===");
                Console.WriteLine($"Zalogowany użytkownik: {messenger.LoggedUsername}");

                Console.WriteLine();
                Console.WriteLine("1. Rejestracja");
                Console.WriteLine("2. Logowanie");
                Console.WriteLine("3. Wylogowanie");
                Console.WriteLine("4. Wyślij wiadomość");
                Console.WriteLine("5. Odbierz wiadomości");
                Console.WriteLine("6. Wyjście");

                Console.Write("\nWybierz opcję: ");

                try
                {
                    if (!int.TryParse(Console.ReadLine(), out int option))
                    {
                        Console.WriteLine("Niepoprawny numer.");
                        continue;
                    }

                    switch (option)
                    {
                        case 1:
                            Console.Write("Podaj nazwę użytkownika: ");
                            string username = Console.ReadLine() ?? "";

                            Console.Write("Podaj hasło: ");
                            string password = Console.ReadLine() ?? "";

                            Console.Write("Potwierdź hasło: ");
                            string confirm = Console.ReadLine() ?? "";

                            messenger.Register(username, password, confirm);

                            Console.WriteLine("Rejestracja zakończona sukcesem.");
                            break;

                        case 2:
                            Console.Write("Podaj nazwę użytkownika: ");
                            string login = Console.ReadLine() ?? "";

                            Console.Write("Podaj hasło: ");
                            string loginPassword = Console.ReadLine() ?? "";

                            messenger.Login(login, loginPassword);

                            Console.WriteLine("Zalogowano.");
                            break;

                        case 3:
                            messenger.Logout();

                            Console.WriteLine("Wylogowano.");
                            break;

                        case 4:
                            Console.Write("Podaj odbiorcę: ");
                            string receiver = Console.ReadLine() ?? "";

                            Console.Write("Treść wiadomości: ");
                            string content = Console.ReadLine() ?? "";

                            messenger.SendMessage(receiver, content);

                            Console.WriteLine("Wiadomość wysłana.");
                            break;

                        case 5:

                            bool back = false;

                            while (!back)
                            {
                                Console.Clear();

                                Console.WriteLine("=== NADAWCY ===");

                                Dictionary<string, int> senders =
                                    messenger.GetSenders();

                                if (senders.Count == 0)
                                {
                                    Console.WriteLine("Brak wiadomości.");
                                    break;
                                }

                                foreach (var sender in senders)
                                {
                                    Console.WriteLine(
                                        $"{sender.Key} ({sender.Value} wiadomości)"
                                    );
                                }

                                Console.WriteLine();
                                Console.WriteLine("Wpisz nazwę nadawcy.");
                                Console.WriteLine("Wpisz BACK aby wrócić.");

                                Console.Write("\nTwój wybór: ");

                                string selectedSender = Console.ReadLine() ?? "";

                                if (selectedSender.ToUpper() == "BACK")
                                {
                                    back = true;
                                    continue;
                                }

                                List<Message> messages =
                                    messenger.GetMessagesFromSender(selectedSender);

                                Console.Clear();

                                Console.WriteLine($"=== Wiadomości od {selectedSender} ===");

                                foreach (Message msg in messages)
                                {
                                    Console.WriteLine(msg.Content);
                                }

                                Console.WriteLine();
                                Console.WriteLine("Naciśnij dowolny klawisz...");
                                Console.ReadKey();
                            }

                            break;

                        case 6:
                            running = false;
                            break;

                        default:
                            Console.WriteLine("Niepoprawna opcja.");
                            break;
                    }
                }
                catch (EmptyUsernameException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (EmptyPasswordException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (PasswordMismatchException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (UserAlreadyExistsException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (UserNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (InvalidPasswordException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (NoLoggedUserException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (EmptyMessageException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (NoMessagesException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Nieoczekiwany błąd: " + ex.Message);
                }

                Console.WriteLine();
                Console.WriteLine("Naciśnij dowolny klawisz...");
                Console.ReadKey();
            }
        }
    }
}