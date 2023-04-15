using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToDoListApi.Models;
using System.Text.Json;
using Azure;

namespace call_own_api_dotnet_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
            private IPublicClientApplication _publicClientApplication = PublicClientApplicationBuilder
                .Create("bc862355-65d0-4f7c-a45e-df6499781b25")
                .WithAuthority("https://login.microsoftonline.com/afa75af5-3425-40e6-a8e5-d64187afed4a")
                .WithRedirectUri("http://localhost:64170")
                .Build();

        private AuthenticationResult _authenticationResult;

        private int _editedToDoId;

        private IPublicClientApplication CreatePublicClient()
        {
            var pca = PublicClientApplicationBuilder.Create("[CLIENT ID HERE]")
                .WithAuthority("https://login.microsoftonline.com/[TENANTID HERE]")
                .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
                .WithLogging((x, y, z) => Debug.WriteLine($"{x} {y}"), LogLevel.Verbose, true)
                .Build();

            BindCache(pca.UserTokenCache, "[USER CACHE FILE HERE]");

            return pca;
        }

        private IPublicClientApplication CreatePublicClientForRuntime()
        {
            var pca = PublicClientApplicationBuilder.Create("[CLIENT ID HERE]")
                .WithAuthority("https://login.microsoftonline.com/[TENANTID HERE]")
                .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows))
                .WithLogging((x, y, z) => Debug.WriteLine($"{x} {y}"), LogLevel.Verbose, true)
                .Build();

            BindCache(pca.UserTokenCache, "[PUT YOUR FILE HERE]");

            return pca;
        }

        private static void BindCache(ITokenCache tokenCache, string file)
        {
            tokenCache.SetBeforeAccess(notificationArgs =>
            {
                notificationArgs.TokenCache.DeserializeMsalV3(File.Exists(file)
                    ? File.ReadAllBytes("[CACHE FILE HERE]")
                    : null);
            });

            tokenCache.SetAfterAccess(notificationArgs =>
            {
                // if the access operation resulted in a cache update
                if (notificationArgs.HasStateChanged)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(file, notificationArgs.TokenCache.SerializeMsalV3());
                }
            });
        }

        private Visibility _toDoListVisibility = Visibility.Visible;
        public Visibility ToDoListVisibility
        {
            get { return _toDoListVisibility; }
            set
            {
                if (value != _toDoListVisibility)
                {
                    _toDoListVisibility = value;
                    OnPropertyChanged("ToDoListVisibility");
                }
            }
        }

        private Visibility _toDoEditVisibility = Visibility.Collapsed;
        public Visibility ToDoEditVisibility
        {
            get { return _toDoEditVisibility; }
            set
            {
                if (value != _toDoEditVisibility)
                {
                    _toDoEditVisibility = value;
                    OnPropertyChanged("ToDoEditVisibility");
                }
            }
        }

        private Visibility _signInButtonVisibility = Visibility.Collapsed;
        public Visibility SignInButtonVisibility
        {
            get { return _signInButtonVisibility; }
            set
            {
                if (value != _signInButtonVisibility)
                {
                    _signInButtonVisibility = value;
                    OnPropertyChanged("SignInButtonVisibility");
                }
            }
        }

        private Visibility _signOutButtonVisibility = Visibility.Visible;
        public Visibility SignOutButtonVisibility
        {
            get { return _signOutButtonVisibility; }
            set
            {
                if (value != _signOutButtonVisibility)
                {
                    _signOutButtonVisibility = value;
                    OnPropertyChanged("SignOutButtonVisibility");
                }
            }
        }

        private ObservableCollection<ToDo> _userToDos = new ObservableCollection<ToDo>();

        public ObservableCollection<ToDo> UserToDos
        {
            get { return _userToDos; }
        }

        public ICommand DeleteToDoCommand { private set; get; }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _editedToDoText;
        public string EditedToDoText
        {
            get { return _editedToDoText; }
            set
            {
                if (value != _editedToDoText)
                {
                    _editedToDoText = value;
                    OnPropertyChanged("EditedToDoText");
                }
            }
        }

        private string _newToDoText = string.Empty;

        public string NewToDoText
        {
            get { return _newToDoText; }
            set
            {
                if (value != _newToDoText)
                {
                    _newToDoText = value;
                    OnPropertyChanged("Name2");
                }
            }
        }

        private async void CreateToDoButton_Click(object sender, RoutedEventArgs e)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:44351");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationResult.AccessToken);

            var newToDo = new ToDo()
            {
                Message = NewToDoText
            };

            var jsonRequest = JsonSerializer.Serialize(newToDo);
            var jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/api/todo", jsonContent);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            newToDo = JsonSerializer.Deserialize<ToDo>(jsonResponse, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            _userToDos.Add(newToDo);
        }
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:44351");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationResult.AccessToken);

            var response = await httpClient.DeleteAsync($"/api/todo{((ToDo)button.DataContext).ID}");


            UserToDos.Remove(UserToDos.First(toDo => toDo.ID == ((ToDo)button.DataContext).ID));

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            _editedToDoId = ((ToDo)button.DataContext).ID;
            EditedToDoText = ((ToDo)button.DataContext).Message;
            ToDoEditVisibility = Visibility.Visible;
            ToDoListVisibility = Visibility.Collapsed;
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ToDoEditVisibility = Visibility.Collapsed;
            ToDoListVisibility = Visibility.Visible;
        }    

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            _authenticationResult = await _publicClientApplication
                .AcquireTokenInteractive(new string[] { "api://5ed29dce-5eef-43e6-a435-2af591ce4cc7/ToDoList.Read", "api://5ed29dce-5eef-43e6-a435-2af591ce4cc7/ToDoList.ReadWrite" })
                .ExecuteAsync();

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:44351");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationResult.AccessToken);

            var serializedToDos = await httpClient!.GetStringAsync("/api/todo");

            var deserializedToDos = JsonSerializer.Deserialize<IEnumerable<ToDo>>(serializedToDos, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            _userToDos.Clear();

            foreach (var toDo in deserializedToDos)
            {
                _userToDos.Add(toDo);
            }

            SignInButtonVisibility = Visibility.Collapsed;
            SignOutButtonVisibility = Visibility.Visible;
            ToDoEditVisibility = Visibility.Collapsed;
            ToDoListVisibility = Visibility.Visible;
        }

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            SignInButtonVisibility = Visibility.Visible;
            SignOutButtonVisibility = Visibility.Collapsed;
            ToDoEditVisibility = Visibility.Collapsed;
            ToDoListVisibility = Visibility.Collapsed;
        }

        private async void SubmitEdit_Click(object sender, RoutedEventArgs e) {
            var editedToDo = new ToDo()
            {
                Message = EditedToDoText
            };

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:44351");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationResult.AccessToken);

            var jsonRequest = JsonSerializer.Serialize(editedToDo);
            var jsonContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PatchAsync($"/api/todo/{_editedToDoId}", jsonContent);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            editedToDo = JsonSerializer.Deserialize<ToDo>(jsonResponse, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            var toDoIndex = _userToDos
                .Select((td, i) => new { td, i })
                .Where(t => t.td.ID == _editedToDoId)
                .Select(t => t.i)
                .First();

            _userToDos[toDoIndex] = editedToDo!;
            ToDoEditVisibility = Visibility.Collapsed;
            ToDoListVisibility = Visibility.Visible;
        }
    }
}

