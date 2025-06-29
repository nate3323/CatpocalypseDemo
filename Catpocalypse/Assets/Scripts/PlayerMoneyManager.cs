 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerMoneyManager : MonoBehaviour
{
    [SerializeField] private float _Money = 200f;
    [SerializeField]
    private AudioSource _moneySound;
    private void Start()
    {
        HUD.UpdatePlayerMoneyDisplay(_Money);


        // All cat types should be able to subsribe this same handler to their OnCatDied events.
        // That function below calls another that checks the cat type.
        CatBase.OnCatDied += OnCatDied;

    }

    public void AddMoney(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("Cannot add money, because the passed in amount is not positive!");
            return;
        }

        _Money += (amount * PlayerDataManager.Instance.Upgrades.RewardUpgrade);
        _moneySound.Play();
        HUD.UpdatePlayerMoneyDisplay(_Money);
    }

    /// <summary>
    /// This function is used to spend money.
    /// </summary>
    /// <remarks>
    /// If the player has enough money, the passed in amount is deducted and this function returns true.
    /// Otherwise, this function will simply return false to indicate there isn't enough money.
    /// 
    /// It also calls the HUD function to update the money display.
    /// </remarks>
    /// <param name="amount">The amount to deduct from the player's money.</param>
    /// <returns>False if funds are not sufficient, or true otherwise.</returns>
    public bool SpendMoney(float amount)
    {
        if (amount > _Money)
        {
            return false;
        }

        _Money -= amount;
        HUD.UpdatePlayerMoneyDisplay(_Money);

        return true;
    }

    private void OnCatDied(object sender, EventArgs e)
    {
        _Money += (sender as CatBase).DistractionReward;
        HUD.UpdatePlayerMoneyDisplay(_Money);
    }

}
