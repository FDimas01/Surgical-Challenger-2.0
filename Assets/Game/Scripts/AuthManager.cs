using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database; 
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI; 

public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser User;
    public DatabaseReference dbReference; 

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField emailRegisterVerifyField; // <--- NOVA VARIÁVEL: Arraste o input de confirmar email aqui
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    // --- VARIÁVEIS DE SELEÇÃO DE PERSONAGEM ---
    [Header("Character Selection")]
    public Toggle maleToggle;   
    public Toggle femaleToggle; 
    public TMP_Text warningCharText;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private string databaseURL = "https://surgical-challenge-default-rtdb.firebaseio.com/"; 

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;

        if (!string.IsNullOrEmpty(databaseURL))
        {
            dbReference = FirebaseDatabase.GetInstance(databaseURL).RootReference;
        }
        else
        {
            Debug.LogError("ERRO: A variável databaseURL está vazia no script AuthManager!");
        }
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    
    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    public void ConfirmCharacterButton()
    {
        StartCoroutine(SaveCharacterChoice());
    }

    private IEnumerator Login(string _email, string _password)
    {
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Falha ao Logar!";
            switch (errorCode)
            {
                case AuthError.MissingEmail: message = "Falta o Email"; break;
                case AuthError.MissingPassword: message = "Falta a Senha"; break;
                case AuthError.WrongPassword: message = "Senha Incorreta"; break;
                case AuthError.InvalidEmail: message = "Email Invalido"; break;
                case AuthError.UserNotFound: message = "Conta inexistente"; break;
            }
            warningLoginText.text = message;
        }
        else
        {
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logado!";

            yield return new WaitForSeconds(1);
            StartCoroutine(CheckIfUserDataExists());
        }
    }

    private IEnumerator CheckIfUserDataExists()
    {
        Debug.Log("1. Iniciando verificação de dados...");

        if (User == null)
        {
            Debug.LogError("ERRO: Usuário é Null!");
            yield break;
        }

        if (dbReference == null)
        {
            Debug.Log("2. Reconectando ao banco...");
            if (!string.IsNullOrEmpty(databaseURL))
            {
                dbReference = FirebaseDatabase.GetInstance(databaseURL).RootReference;
            }
        }

        Debug.Log("3. Consultando o banco de dados...");
        var task = dbReference.Child("users").Child(User.UserId).Child("gender").GetValueAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);

        Debug.Log("4. Consulta finalizada!");

        if (task.Exception != null)
        {
            Debug.LogWarning("ERRO no Banco: " + task.Exception);
            UIManager.instance.MenuScreen();
        }
        else if (task.Result.Exists)
        {
            Debug.Log("RESULTADO: Personagem já existe. Indo para Menu.");
            UIManager.instance.MenuScreen();
        }
        else
        {
            Debug.Log("RESULTADO: Novo usuário. Indo para Seleção de Personagem.");
            
            if (UIManager.instance.charSelectUI == null)
            {
                Debug.LogError("ERRO CRÍTICO: Você esqueceu de arrastar a tela 'charSelectUI' no Inspector do UIManager!");
            }
            else
            {
                UIManager.instance.CharacterSelectScreen();
            }
        }
        
        confirmLoginText.text = ""; 
    }

    private IEnumerator SaveCharacterChoice()
    {
        string genderChoice = "";

        if (maleToggle.isOn) genderChoice = "Male";
        else if (femaleToggle.isOn) genderChoice = "Female";
        else
        {
            if(warningCharText) warningCharText.text = "Selecione um personagem!";
            yield break; 
        }

        Task DBTask = dbReference.Child("users").Child(User.UserId).Child("gender").SetValueAsync(genderChoice);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning("Erro ao salvar dados: " + DBTask.Exception);
            if(warningCharText) warningCharText.text = "Erro ao salvar!";
        }
        else
        {
            UIManager.instance.MenuScreen();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            warningRegisterText.text = "Falta o Nome";
        }
        // --- NOVA VERIFICAÇÃO DE EMAIL ---
        else if (_email != emailRegisterVerifyField.text)
        {
            warningRegisterText.text = "Emails Discrepantes!";
        }
        // ---------------------------------
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Senhas incompativeis!";
        }
        else 
        {
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Falha ao Registrar!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail: message = "Falta o Email"; break;
                    case AuthError.MissingPassword: message = "Falta a Senha"; break;
                    case AuthError.WeakPassword: message = "Senha Fraca"; break;
                    case AuthError.EmailAlreadyInUse: message = "Email Ja Em Uso"; break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                User = RegisterTask.Result.User;
                if (User != null)
                {
                    UserProfile profile = new UserProfile{DisplayName = _username};
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        warningRegisterText.text = "Falha Ao Inserir Nome!";
                    }
                    else
                    {
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                    }
                }
            }
        }
    }

    public void LogOut()
    {
        if (auth != null) auth.SignOut();
        User = null;

        if(emailLoginField) emailLoginField.text = "";
        if(passwordLoginField) passwordLoginField.text = "";
        if(confirmLoginText) confirmLoginText.text = "";

        UIManager.instance.LoginScreen();
        Debug.Log("Usuário desconectado.");
    }
}