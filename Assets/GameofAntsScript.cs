using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameofAntsScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> cells;
    public List<KMSelectable> arrows;
    public List<KMSelectable> buttons;
    public Renderer[] crends;
    public Renderer[] arends;
    public Renderer[] brends;
    public Transform[] ants;
    public Material[] io;

    private bool[][,] states = new bool[4][,];
    private List<int[]> antstart = new List<int[]> { };
    private bool[,] grid = new bool[5, 5];
    private readonly int[][] antdir = new int[25][] { new int[0] { }, new int[1] {2}, new int[1] {2}, new int[1] {2}, new int[0] { }, new int[1] {1}, new int[2] {1, 2}, new int[3] {1, 2, 3}, new int[2] {2, 3}, new int[1] {3}, new int[1] {1}, new int[3] {0, 1, 2}, new int[4] {0, 1, 2, 3}, new int[3] {0, 2, 3}, new int[1] {3}, new int[1] {1}, new int[2] {0, 1}, new int[3] {0, 1, 3}, new int[2] {0, 3}, new int[1] {3}, new int[0] { }, new int[1] {0}, new int[1] {0}, new int[1] {0}, new int[0] { } };
    private List<int[]> antpos = new List<int[]> { };
    private bool[,] subcells = new bool[5, 5];
    private List<int[]> subants = new List<int[]> { };
    private int antrot;
    private bool antselect;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private bool[,] G(bool[,] s)
    {
        bool[,] z = new bool[5, 5];
        for(int i = 0; i < 25; i++)
        {
            int p = 0;
            for(int j = 0; j < 3; j++)
            {
                int x = (i / 5) + (j - 1);
                if (x < 0 || x > 4)
                    continue;
                for(int k = 0; k < 3; k++)
                {
                    int y = (i % 5) + (k - 1);
                    if (y < 0 || y > 4)
                        continue;
                    if (s[x, y])
                        p++;
                }
            }
            if (p == 3 || (p == 4 && s[i / 5, i % 5]))
                z[i / 5, i % 5] = true;
        }
        return z;
    }

    private void Track(int i)
    {
        bool[,] copy = new bool[5, 5];
        for (int j = 0; j < 25; j++)
            copy[j / 5, j % 5] = grid[j / 5, j % 5];
        states[i] = copy;
    }

    private void Move(int[] ant, bool toggle)
    {
        if (toggle)
            grid[ant[0], ant[1]] ^= true;
        switch (ant[2])
        {
            case 0: ant[0]--; break;
            case 1: ant[1]++; break;
            case 2: ant[0]++; break;
            default: ant[1]--; break;
        }
    }

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        for(int i = 0; i < 25; i++)
        {
            bool r = Random.Range(0, 2) > 0;
            grid[i / 5, i % 5] = r;
            subcells[i / 5, i % 5] = r;
            crends[i].material = io[r ? 0 : 1];
        }
        Track(0);
        Debug.LogFormat("[Game of Ants #{0}] The initial state of the grid is:\n[Game of Ants #{0}] {1}", moduleID, string.Join("\n[Game of Ants #" + moduleID + "] ", Enumerable.Range(0, 5).Select(x => string.Join("", Enumerable.Range(0, 5).Select(y => grid[x, y] ? "\u25a1" : "\u25a0").ToArray())).ToArray()));
        grid = G(grid);
        Track(1);
        Debug.LogFormat("[Game of Ants #{0}] The life phase yields:\n[Game of Ants #{0}] {1}", moduleID, string.Join("\n[Game of Ants #" + moduleID + "] ", Enumerable.Range(0, 5).Select(x => string.Join("", Enumerable.Range(0, 5).Select(y => grid[x, y] ? "\u25a1" : "\u25a0").ToArray())).ToArray()));
        int antnum = Random.Range(2, 6);
        List<int> antinit = new List<int> { 1, 2, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21, 22, 23};
        for(int i = 0; i < antnum; i++)
        {
            int r = antinit.PickRandom();
            arends[r].enabled = true;
            antinit.Remove(r);
            int[] s = new int[3] { r / 5, r % 5, antdir[r].PickRandom() };
            ants[r].localEulerAngles = new Vector3(0, (s[2] + 2) * 90, 0);
            antpos.Add(s);
            subants.Add(new int[3] { s[0], s[1], s[2]});
            antstart.Add(new int[3] { s[0], s[1], s[2] });
        }
        int[] antcoord = new int[antnum];
        bool[] antshare = new bool[antnum];
        string[] antlog = new string[antnum];
        for (int i = 0; i < antnum; i++)
            antlog[i] = "ABCDE"[antpos[i][1]].ToString() + (antpos[i][0] + 1).ToString() + ":";
        while (true)
        {
            for (int i = 0; i < antnum; i++)
            {
                Move(antpos[i], true);
                antcoord[i] = (antpos[i][0] * 6) + antpos[i][1];
                antlog[i] += "URDL"[antpos[i][2]].ToString();
            }
            for (int i = 0; i < antnum; i++)
                antshare[i] = antcoord.Where((x, k) => k != i && x == antcoord[i]).Any() && antpos.Select(x => x[2]).Where((x, k) => k > i && antcoord[k] == antcoord[i]).All(x => x != antpos[i][2]);
            while (antshare.Any(x => x))
            {
                for (int i = 0; i < antnum; i++)
                    if (antshare[i])
                    {
                        Move(antpos[i], false);
                        antcoord[i] = (antpos[i][0] * 6) + antpos[i][1];
                        antlog[i] += "-";
                    }
                for (int i = 0; i < antnum; i++)
                    antshare[i] = antcoord.Where((x, k) => k != i && x == antcoord[i]).Any() && antpos.Select(x => x[2]).Where((x, k) => k > i && antcoord[k] == antcoord[i]).All(x => x != antpos[i][2]);
            }
            if (Enumerable.Range(0, antnum * 2).Select(x => antpos[x / 2][x % 2]).Any(x => x < 0 || x > 4))
                break;
            for (int i = 0; i < antnum; i++)
            {
                int[] ant = antpos[i];
                if (ant[0] >= 0 && ant[0] < 5 && ant[1] >= 0 && ant[1] < 5)
                {
                    ant[2] += grid[ant[0], ant[1]] ? 1 : 3;
                    ant[2] %= 4;
                }
            } 
        }
        for(int i = 0; i < antnum; i++)
        {
            int[] ant = antpos[i];
            antlog[i] += ":";
            if (ant[0] < 0)
            {
                antpos[i][0] = -1;
                antpos[i][2] = 0;
                antlog[i] += "OUT N" + (ant[1] + 1).ToString();
            }
            else if (ant[0] > 4)
            {
                antpos[i][0] = 5;
                antpos[i][2] = 2;
                antlog[i] += "OUT S" + (ant[1] + 1).ToString();
            }
            else if (ant[1] < 0)
            {
                antpos[i][1] = -1;
                antpos[i][2] = 3;
                antlog[i] += "OUT W" + (ant[0] + 1).ToString();
            }
            else if (ant[1] > 4)
            {
                antpos[i][1] = 5;
                antpos[i][2] = 1;
                antlog[i] += "OUT E" + (ant[0] + 1).ToString();
            }
            else
                antlog[i] += "ABCDE"[ant[1]] + (ant[0] + 1).ToString();
        }
        antlog = antlog.OrderBy(x => x).ToArray();
        Debug.LogFormat("[Game of Ants #{0}] The ants travel along the following paths:\n[Game of Ants #{0}] {1}", moduleID, string.Join("\n[Game of Ants #" + moduleID + "] ", antlog));
        Track(2);
        Debug.LogFormat("[Game of Ants #{0}] The ant phase yields:\n[Game of Ants #{0}] {1}", moduleID, string.Join("\n[Game of Ants #" + moduleID + "] ", Enumerable.Range(0, 5).Select(x => string.Join("", Enumerable.Range(0, 5).Select(y => grid[x, y] ? "\u25a1" : "\u25a0").ToArray())).ToArray()));
        grid = G(grid);
        Track(3);
        Debug.LogFormat("[Game of Ants #{0}] The final state of the grid is:\n[Game of Ants #{0}] {1}", moduleID, string.Join("\n[Game of Ants #" + moduleID + "] ", Enumerable.Range(0, 5).Select(x => string.Join("", Enumerable.Range(0, 5).Select(y => grid[x, y] ? "\u25a1" : "\u25a0").ToArray())).ToArray()));
        for(int i = 0; i < antnum - 1; i++)
            if (antpos.Skip(i + 1).Any(x => x.SequenceEqual(antpos[i])))
            {
                antpos.RemoveAt(i);
                i--;
            }
        foreach (KMSelectable cell in cells)
        {
            int b = cells.IndexOf(cell);
            cell.OnInteract = delegate ()
            {
                if (!moduleSolved)
                {
                    cell.AddInteractionPunch(0.2f);
                    if (antselect)
                    {                       
                        int[] a = new int[3] { b / 5, b % 5, antrot};
                        int[] c = new int[3];
                        if (subants.Any(x => x.SequenceEqual(a)))
                        {
                            c = subants.First(x => x.SequenceEqual(a));
                            subants.Remove(c);
                            Audio.PlaySoundAtTransform("AntOff", cell.transform);
                            arends[b].enabled = false;
                        }
                        else
                        {
                            Audio.PlaySoundAtTransform("AntOn", cell.transform);
                            c = subants.FirstOrDefault(x => x[0] == a[0] && x[1] == a[1]);
                            if (c == null)
                                subants.Add(a);
                            else
                                c[2] = antrot;
                            ants[b].localEulerAngles = new Vector3(0, (antrot + 2) * 90, 0);
                            arends[b].enabled = true;
                        }
                    }
                    else
                    {
                        subcells[b / 5, b % 5] ^= true;
                        Audio.PlaySoundAtTransform(subcells[b / 5, b % 5] ? "CellOn" : "CellOff", cell.transform);
                        crends[b].material = io[subcells[b / 5, b % 5] ? 0 : 1];
                    }
                }
                return false;
            };
        }
        foreach(KMSelectable arrow in arrows)
        {
            int b = arrows.IndexOf(arrow);
            arrow.OnInteract = delegate ()
            {
                if (!antselect)
                {
                    antselect = true;
                    brends[0].material = io[0];
                    brends[1].material = io[1];
                }
                int[] a = new int[3] { 0, 0, b / 5};
                antrot = a[2];
                ants[25].transform.localEulerAngles = new Vector3(0, (antrot + 2) * 90, 0);
                switch(a[2])
                {
                    case 0: a[0] = -1; a[1] = b % 5; break;
                    case 1: a[0] = b % 5; a[1] = 5; break;
                    case 2: a[0] = 5; a[1] = b % 5; break;
                    default: a[0] = b % 5; a[1] = -1; break;
                }
                if (subants.Any(x => x.SequenceEqual(a)))
                {
                    Audio.PlaySoundAtTransform("AntOff", arrow.transform);
                    subants.Remove(subants.First(x => x.SequenceEqual(a)));
                    arends[b + 25].enabled = false;
                    crends[b + 25].material = io[1];
                }
                else
                {
                    Audio.PlaySoundAtTransform("AntOn", arrow.transform);
                    subants.Add(a);
                    arends[b + 25].enabled = true;
                    crends[b + 25].material = io[0];
                }
                return false;
            };
        }
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract = delegate ()
            {
                if (!moduleSolved)
                {
                    button.AddInteractionPunch();
                    if(b == 0 && antselect)
                    {
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                        antrot++;
                        antrot %= 4;
                        ants[25].transform.localEulerAngles = new Vector3(0, (antrot + 2) * 90, 0);
                    }
                    else
                    {
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                        switch (b)
                        {
                            case 0:
                                antselect = true;
                                brends[0].material = io[0];
                                brends[1].material = io[1];
                                break;
                            case 1:
                                antselect = false;
                                brends[0].material = io[1];
                                brends[1].material = io[0];
                                break;
                            case 2:
                                for(int i = 0; i < 25; i++)
                                {
                                    subcells[i / 5, i % 5] = false;
                                    crends[i].material = io[1];
                                    arends[i].enabled = false;
                                }
                                subants.Clear();
                                for(int i = 25; i < 45; i++)
                                {
                                    crends[i].material = io[1];
                                    arends[i].enabled = false;
                                }
                                break;
                            case 3:
                                subants.Clear();
                                for(int i = 0; i < 25; i++)
                                {
                                    bool s = states[0][i / 5, i % 5];
                                    subcells[i / 5, i % 5] = s;
                                    crends[i].material = io[s ? 0 : 1];
                                    int[] c = antstart.FirstOrDefault(x => x[0] == i / 5 && x[1] == i % 5);
                                    if (c == null)
                                        arends[i].enabled = false;
                                    else
                                    {
                                        c = new int[3] { c[0], c[1], c[2] };
                                        if (subants.All(x => !x.SequenceEqual(c)))
                                            subants.Add(c);
                                        arends[i].enabled = true;
                                        ants[i].localEulerAngles = new Vector3(0, (c[2] + 2) * 90, 0);
                                    }
                                }
                                for (int i = 25; i < 45; i++)
                                {
                                    crends[i].material = io[1];
                                    arends[i].enabled = false;
                                }
                                break;
                            default:
                                if (Enumerable.Range(0, 25).All(x => subcells[x / 5, x % 5] == states[3][x / 5, x % 5]) && subants.Count == antpos.Count && subants.All(x => antpos.Any(y => x.SequenceEqual(y))))
                                {
                                    module.HandlePass();
                                    moduleSolved = true;
                                    AnimGrid(0);
                                    for(int i = 0; i < 25; i++)
                                    {
                                        int[] c = antstart.FirstOrDefault(x => x[0] == i / 5 && x[1] == i % 5);
                                        if (c == null)
                                            arends[i].enabled = false;
                                        else
                                        {
                                            arends[i].enabled = true;
                                            ants[i].localEulerAngles = new Vector3(0, (c[2] + 2) * 90, 0);
                                        }
                                    }
                                    for (int i = 25; i < 45; i++)
                                    {
                                        crends[i].material = io[1];
                                        arends[i].enabled = false;
                                    }
                                    Audio.PlaySoundAtTransform("Solve", transform);
                                    StartCoroutine(Solve());
                                }
                                else
                                    module.HandleStrike();
                                break;
                        }
                    }
                }
                return false;
            };
        }
        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        int i = 0;
        while (module.gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1);
            i ^= 1;
            brends[2].material = io[i];
        }
    }

    private void AnimGrid(int g)
    {
        for(int i = 0; i < 25; i++)
            crends[i].material = io[states[g][i / 5, i % 5] ? 0 : 1];
    }

    private IEnumerator Solve()
    {
        yield return new WaitForSeconds(0.333f);
        AnimGrid(1);
        yield return new WaitForSeconds(0.633f);
        AnimGrid(2);
        for (int i = 0; i < 25; i++)
        {
            int[] c = subants.FirstOrDefault(x => x[0] == i / 5 && x[1] == i % 5);
            if (c == null)
                arends[i].enabled = false;
            else
            {
                arends[i].enabled = true;
                ants[i].localEulerAngles = new Vector3(0, (c[2] + 2) * 90, 0);
            }
        }
        for (int i = 25; i < 45; i++)
        {
            int[] a = new int[3] {0, 0, (i - 25) / 5};
            switch (a[2])
            {
                case 0: a[0] = -1; a[1] = i % 5; break;
                case 1: a[0] = i % 5; a[1] = 5; break;
                case 2: a[0] = 5; a[1] = i % 5; break;
                default: a[0] = i % 5; a[1] = -1; break;
            }
            bool b = subants.Any(x => x.SequenceEqual(a));
            crends[i].material = io[b ? 0 : 1];
            arends[i].enabled = b;
        }
        yield return new WaitForSeconds(0.633f);
        AnimGrid(3);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <a-e><1-5> [Selects cell. Chain with spaces.] | !{0} <NESW><1-5> [Selects arrow in the position from left-to-right or top-to-bottom poiting in the direction given.] | !{0} set cell/ant <NESW> [Switches between toggling cells and placing ants. Adding a direction to a set ant command rotates the ant selector.] | !{0} clear | !{0} reset | !{0} submit";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.Split(' ');
        switch (commands[0].ToUpperInvariant())
        {
            case "SET":
                if(commands.Length < 2)
                {
                    yield return "sendtochaterror!f No option to set given.";
                    yield break;
                }
                if(commands[1].ToUpperInvariant() == "CELL")
                {
                    yield return null;
                    buttons[1].OnInteract();
                    yield break;
                }
                if (commands[1].ToUpperInvariant() == "ANT")
                {
                    yield return null;
                    if(commands.Length > 2)
                    {
                        int d = "NESW".IndexOf(commands[2].ToUpperInvariant());
                        if(d < 0)
                        {
                            yield return "sendtochaterror!f " + commands[3] + " is not a valid direction.";
                            yield break;
                        }
                        while(antrot != d)
                        {
                            yield return new WaitForSeconds(0.1f);
                            buttons[0].OnInteract();
                        }
                    }
                    else if(!antselect)
                        buttons[0].OnInteract();
                    yield break;
                }
                yield break;
            case "CLEAR":
                yield return null;
                buttons[2].OnInteract();
                yield break;
            case "RESET":
                yield return null;
                buttons[3].OnInteract();
                yield break;
            case "SUBMIT":
                yield return null;
                buttons[4].OnInteract();
                yield break;
        }
        if(command.Length == 1 && commands[0].Length == 2 && !"abcde".Contains(commands[0].ToString()))
        {
            int d = 0;
            if ("NESW".Contains(command[0].ToString()))
            {
                d += "NESW".IndexOf(command[0].ToString()) * 5;
                if ("12345".Contains(command[1].ToString()))
                {
                    d += command[1] - '1';
                    yield return null;
                    arrows[d].OnInteract();
                    yield break;
                }
                else
                    yield return "sendtochateroor!f Invalid position entered,";
            }
            else
            {
                yield return "sendtochateroor!f " + command + " is not a valid cell or arrow.";
                yield break;
            }
        }
        List<int> c = new List<int> { };
        for(int i = 0; i < commands.Length; i++)
        {
            if(commands[i].Length == 2)
            {
                int d = 0;
                if ("abcde".Contains(commands[i]))
                {
                    d = "abcde".IndexOf(commands[i][0].ToString());
                    if ("12345".Contains(commands[i][1].ToString()))
                    {
                        d += (commands[i][1] - '1') * 5;
                        c.Add(d);
                    }
                    else
                        yield return "sendtochaterror!f " + commands[i] + " is not a valid cell.";
                }
                else
                    yield return "sendtochaterror!f " + commands[i] + " is not a valid cell.";
            }
            else
                yield return "sendtochaterror!f " + commands[i] + " is not a valid cell.";
        }
        for(int i = 0; i < commands.Length; i++)
        {
            yield return null;
            cells[c[i]].OnInteract();
        }
    }
}
