using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnAisPawn : MonoBehaviour
{

    // 駒の生成を行うスクリプト。並び順はシャッフル

    public List<Image> pawn_prefabs;
    public Transform myTransform;

    public static List<Image> hands;  // 手札
    public static List<Image> deck;   // 山札

    void Awake()
    {
        List<int> chosen_pawn = new List<int>();
        hands = new List<Image>();
        deck = new List<Image>();


        for (int i = 0; i < pawn_prefabs.Count; i++)
        {
            int randomIndex = Random.Range(0, pawn_prefabs.Count);

            while (chosen_pawn.Contains(randomIndex))
            {
                // randomIndexが chosen_pawnの中に既に含まれていたらやり直し
                randomIndex = Random.Range(0, pawn_prefabs.Count);
            }
            chosen_pawn.Add(randomIndex);


            if (i < 3)
            {

                hands.Add(pawn_prefabs[randomIndex]);
                pawn_prefabs[randomIndex].transform.position = myTransform.position + new Vector3(4.5f + (i * 1.55f), -2.5f, 0);
                Vector3 pos = pawn_prefabs[randomIndex].transform.position;

                Instantiate(pawn_prefabs[randomIndex], pos, Quaternion.identity, myTransform);
            }
            else
            {

                deck.Add(pawn_prefabs[randomIndex]);
                pawn_prefabs[randomIndex].transform.position = myTransform.position + new Vector3(4.5f + (i * 1.55f), -2.5f, 0);

                Vector3 newPosition = myTransform.position + new Vector3(i, 0, 0);

                Instantiate(pawn_prefabs[randomIndex], newPosition, Quaternion.identity, myTransform);
            }
        }

        MiniMaxAI.SetHands(hands);

        MiniMaxAI.SetDeck(deck);
    }

}
