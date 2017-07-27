using Chatty.Client.Annotations;
using Chatty.Protocol.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace Chatty.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ChattyClient client = new ChattyClient();

        public class Contact: INotifyPropertyChanged
        {
            private string name;
            private string id;
            private string messages;
            private bool flash;

            public string Name
            {
                get { return name; }
                set
                {
                    if (value == name) return;
                    name = value;
                    OnPropertyChanged1();
                }
            }
            public string Id
            {
                get { return id; }
                set
                {
                    if (value == id) return;
                    id = value;
                    OnPropertyChanged1();
                }
            }
            public string Messages
            {
                get { return messages; }
                set
                {
                    if (value == messages) return;
                    messages = value;
                    OnPropertyChanged1();
                }
            }
            public bool   Flash
            {
                get { return flash; }
                set
                {
                    if (value == flash) return;
                    flash = value;
                    OnPropertyChanged1();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class ChatViewModel
        {
            public ObservableCollection<Contact> Contacts { get; }
            
            public ChatViewModel()
            {
                Contacts = new ObservableCollection<Contact>();
            }
        }

        private readonly ChatViewModel viewModel = new ChatViewModel();

        public MainWindow()
        {
            DataContext = viewModel;

            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            ChattyClientProtocol protocol = new ChattyClientProtocol(client)
            {
                uOnChatMessageReceived = (senderId, msg) =>
                {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        Contact contact = viewModel.Contacts.FirstOrDefault(t => t.Id == senderId);

                        contact.Messages += $"\n[{DateTime.Now}] from {viewModel.Contacts.First(t => t.Id == senderId).Name}: {msg}";
                        contact.Flash = true;
                    }));
                },
                uOnActiveClientsReceived = clients =>
                {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        List<Contact> currentActiveContacts = new List<Contact>(clients.Count / 2);

                        for (int i = 0; i < clients.Count; i += 2)
                        {
                            if (clients[i + 1] == tbName.Text)
                                continue;

                            Contact possibleValue = viewModel.Contacts.FirstOrDefault(t => t.Id == clients[i]);
                            currentActiveContacts.Add(new Contact
                            {
                                Id       = clients[i],
                                Name     = clients[i + 1],
                                Messages = possibleValue == null ? String.Empty : possibleValue.Messages
                            });
                        }

                        viewModel.Contacts.Clear();

                        foreach (Contact cont in currentActiveContacts)
                            viewModel.Contacts.Add(cont);
                    }));
                },
                uOnDisconnectReceived = () =>
                {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        Title = "Chatty: disconnected by server due inactivity";
                        btnLogin.IsEnabled = true;

                        tbServerPort.Visibility = Visibility.Visible;
                        tbServerIp.Visibility   = Visibility.Visible;
                        tbName.Visibility       = Visibility.Visible;
                        btnLogin.Visibility     = Visibility.Visible;

                        viewModel.Contacts.Clear();
                    }));
                },
                uOnRegisterReceivedUser = () =>
                {
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        Title = $"Chatty: connected as {tbName.Text}";
                        btnLogin.IsEnabled = false;
                    }));
                }
            };

            IPAddress ip;
            if (!IPAddress.TryParse(tbServerIp.Text, out ip))
            {
                MessageBox.Show("Not a valid ip", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int port;
            if (!Int32.TryParse(tbServerPort.Text, out port))
            {
                MessageBox.Show("Not a valid port number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!client.Start(protocol, tbName.Text, ip, port))
            {
                MessageBox.Show("Failed to connect to server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            tbMessage.IsEnabled      = true;
            btnSendMessage.IsEnabled = true;

            tbServerPort.Visibility = Visibility.Collapsed;
            tbServerIp.Visibility   = Visibility.Collapsed;
            tbName.Visibility       = Visibility.Collapsed;
            btnLogin.Visibility     = Visibility.Collapsed;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.DisconnectFromServer();
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (lvContacts.SelectedItem == null)
                return;

            Contact contact = (Contact) lvContacts.SelectedItem;

            contact.Flash = false;
            contact.Messages += $"\n[{DateTime.Now}] from {tbName.Text} : {tbMessage.Text}";

            client.SendMessageToClient(contact.Id, tbMessage.Text);

            tbMessage.Text = string.Empty;
        }
    }
}
