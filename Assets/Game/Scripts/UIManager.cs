using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    [Header("Auth Screens")]
    public GameObject loginUI;
    public GameObject registerUI;
    
    public GameObject charSelectUI; 

    [Header("Main Menus")]
    public GameObject menuUI; 
    
    [Header("Menu Sub-Screens")] 
    public GameObject levelUI;       
    public GameObject helpUI;
    
    // --- MUDANÇA: "Conquistas" agora é "Perfil" ---
    public GameObject profileUI; // Arraste sua tela de Perfil aqui (antiga conquistas)
    
    [Header("Game Levels")]
    public GameObject level1UI; 
    public GameObject level2UI; 
    public GameObject level3UI; 

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != null) Destroy(this);
    }

    // Função chamada ao Sair da conta (LogOut)
    public void LoginScreen() 
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
        if(charSelectUI != null) charSelectUI.SetActive(false); 
        if(menuUI != null) menuUI.SetActive(false);
        
        // --- AQUI ESTÁ A CORREÇÃO ---
        // Garante que o perfil feche quando você for para a tela de login
        if(profileUI != null) profileUI.SetActive(false);
    }

    public void RegisterScreen() 
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
        if(charSelectUI != null) charSelectUI.SetActive(false);
        if(menuUI != null) menuUI.SetActive(false);
        if(profileUI != null) profileUI.SetActive(false);
    }

    public void CharacterSelectScreen()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        menuUI.SetActive(false);
        if(charSelectUI != null) charSelectUI.SetActive(true); 
    }

    public void MenuScreen() 
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        if(charSelectUI != null) charSelectUI.SetActive(false); 

        if (levelUI != null) levelUI.SetActive(false);
        if (helpUI != null) helpUI.SetActive(false);
        
        // Fecha o perfil se estiver voltando dele para o menu
        if(profileUI != null) profileUI.SetActive(false);

        // Fecha fases
        if (level1UI != null) level1UI.SetActive(false);
        if (level2UI != null) level2UI.SetActive(false);
        if (level3UI != null) level3UI.SetActive(false);

        menuUI.SetActive(true); 
    }

    public void LevelSelectionScreen()
    {
        menuUI.SetActive(false);
        if (level1UI != null) level1UI.SetActive(false);
        if (level2UI != null) level2UI.SetActive(false);
        if (level3UI != null) level3UI.SetActive(false);
        if (levelUI != null) levelUI.SetActive(true);
    }

    // --- NOVA FUNÇÃO PARA O BOTÃO DO MENU ---
    // Use essa função no botão que antes abria "Conquistas"
    public void ProfileScreen()
    {
        menuUI.SetActive(false);
        if (profileUI != null) profileUI.SetActive(true);
    }

    public void HelpScreen()
    {
        menuUI.SetActive(false);
        if (helpUI != null) helpUI.SetActive(true);
    }

    // Funções de abrir fases continuam iguais
    public void OpenLevel1() { levelUI.SetActive(false); if (level1UI != null) level1UI.SetActive(true); }
    public void OpenLevel2() { levelUI.SetActive(false); if (level2UI != null) level2UI.SetActive(true); }
    public void OpenLevel3() { levelUI.SetActive(false); if (level3UI != null) level3UI.SetActive(true); }
}