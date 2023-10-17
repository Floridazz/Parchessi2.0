using System.Collections;

using System.Collections.Generic;
using _Scripts.Player;
using Shun_Unity_Editor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HandDice : MonoBehaviour, ITargeter<HandDice>
{
    private PlayerDiceHand _playerDiceHand;
    [SerializeField, ShowImmutable] DiceDescription _diceDescription;
    
    public void Initialize(PlayerDiceHand playerDiceHand, DiceDescription diceDescription)
    {
        _playerDiceHand = playerDiceHand;
        _diceDescription = diceDescription;
    }
    private int GetDice()
    {
        if (_diceDescription == null) return -1;
        
        return Random.Range(_diceDescription.DiceLowerRange, _diceDescription.DiceUpperRange);
    }
    
    public TextMeshProUGUI textMeshPro;
    
    public void PrintDice()
    {
        
        var diceRes = this.GetDice().ToString();
        
        textMeshPro.text = diceRes;
        Debug.Log("Dice roll result is: "+diceRes);
    }

    
}
