using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MazeCell : MonoBehaviour
{
    [SerializeField]
    private GameObject leftWall;
    [SerializeField]
    private GameObject rightWall;
    [SerializeField]
    private GameObject frontWall;
    [SerializeField]
    private GameObject backWall;
    [SerializeField]
    private GameObject unvisitedBlock;
    [SerializeField]
    private GameObject lockerPrefab;

    public bool IsVisited { get; private set; }

    public void Visit()
    {
        IsVisited = true;
        unvisitedBlock.SetActive(false);
    }

    public void ClearLeftWall()
    {
        leftWall.SetActive(false);
    }

    public void ClearRightWall()
    {
        rightWall.SetActive(false);
    }

    public void ClearFrontWall()
    {
        frontWall.SetActive(false);
    }

    public void ClearBackWall()
    {
        backWall.SetActive(false);
    }

    // Vrátí seznam aktivních zdí
    public List<GameObject> GetActiveWalls()
    {
        List<GameObject> activeWalls = new List<GameObject>();
        if (leftWall.activeSelf) activeWalls.Add(leftWall);
        if (rightWall.activeSelf) activeWalls.Add(rightWall);
        if (frontWall.activeSelf) activeWalls.Add(frontWall);
        if (backWall.activeSelf) activeWalls.Add(backWall);
        return activeWalls;
    }

    // Vytvoří locker u náhodné aktivní zdi s rotací
    public void CreateLocker()
    {
        var activeWalls = GetActiveWalls();
        if (activeWalls.Count == 0) return; // Pokud nejsou žádné zdi, locker nevytvoříme

        // Náhodně vybereme zeď
        GameObject selectedWall = activeWalls[Random.Range(0, activeWalls.Count)];

        // Pozice lockeru u zdi
        Vector3 lockerPos = transform.position + new Vector3(0, 3f, 0); // Výchozí Y pozice
        Quaternion lockerRot = Quaternion.identity;

        if (selectedWall == leftWall)
        {
            lockerPos += new Vector3(-3, 0, 0); // U levé zdi
            lockerRot = Quaternion.Euler(0, 90, 0); // Otočení směrem doprava
        }
        else if (selectedWall == rightWall)
        {
            lockerPos += new Vector3(3, 0, 0); // U pravé zdi
            lockerRot = Quaternion.Euler(0, -90, 0); // Otočení směrem doleva
        }
        else if (selectedWall == frontWall)
        {
            lockerPos += new Vector3(0, 0, 3); // U přední zdi
            lockerRot = Quaternion.Euler(0, 180, 0); // Otočení směrem dozadu
        }
        else if (selectedWall == backWall)
        {
            lockerPos += new Vector3(0, 0, -3); // U zadní zdi
            lockerRot = Quaternion.Euler(0, 0, 0); // Otočení směrem dopředu
        }

        // Vytvoření lockeru
        GameObject locker = Instantiate(lockerPrefab, lockerPos, lockerRot);
        locker.transform.SetParent(transform);
    }
}