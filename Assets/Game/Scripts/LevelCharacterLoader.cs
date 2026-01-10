using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

public class LevelCharacterLoader : MonoBehaviour
{
    [Header("Personagens na Cena")]
    public GameObject maleCharacter;   // Arraste o boneco Homem
    public GameObject femaleCharacter; // Arraste o boneco Mulher

    [Header("Configuração")]
    // URL DO FIREBASE AQUI:
    private string databaseURL = "https://surgical-challenge-default-rtdb.firebaseio.com/"; 

    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private FirebaseUser user;

    void OnEnable() // Roda toda vez que a tela de níveis é ativada
    {
        // 1. Garante que os dois comecem desligados para não aparecer o errado
        if(maleCharacter) maleCharacter.SetActive(false);
        if(femaleCharacter) femaleCharacter.SetActive(false);

        // 2. Inicializa e busca
        InitializeFirebaseAndFetch();
    }

    void InitializeFirebaseAndFetch()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        if (user != null)
        {
            // Conecta no banco
            if (!string.IsNullOrEmpty(databaseURL))
            {
                dbReference = FirebaseDatabase.GetInstance(databaseURL).RootReference;
                // Inicia a busca dos dados
                StartCoroutine(FetchGenderAndSpawn());
            }
        }
        else
        {
            Debug.LogError("Usuário não logado! Não é possível carregar o personagem.");
        }
    }

    private IEnumerator FetchGenderAndSpawn()
    {
        // Busca: users -> ID -> gender
        var task = dbReference.Child("users").Child(user.UserId).Child("gender").GetValueAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("Erro ao baixar dados do personagem: " + task.Exception);
        }
        else if (task.Result.Exists)
        {
            string gender = task.Result.Value.ToString();
            Debug.Log("Gênero baixado do Firebase: " + gender);

            // Ativa o boneco correto
            if (gender == "Male")
            {
                if(maleCharacter) maleCharacter.SetActive(true);
            }
            else if (gender == "Female")
            {
                if(femaleCharacter) femaleCharacter.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Nenhum gênero encontrado no banco. Ativando padrão (Male).");
            if(maleCharacter) maleCharacter.SetActive(true);
        }
    }
}