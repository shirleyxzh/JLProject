using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

[RequireComponent(typeof(Parallax))]
public class Connector : MonoBehaviour
{
    public Parallax SectionA;
    public Parallax SectionB;

    public bool IsHorz { get; private set; }
    public Parallax section_a { get; private set; }
    public Parallax section_b { get; private set; }

    private void Start()
    {
        // determine which type of connector: horz/vert
        section_a = SectionA.bounds.min.x < SectionB.bounds.min.x ? SectionA : SectionB;
        section_b = section_a == SectionA ? SectionB : SectionA;
        IsHorz = section_a.bounds.max.x < section_b.bounds.min.x;
        if (!IsHorz)
        {
            // section A bottom / section B top
            section_a = SectionA.bounds.min.y < SectionB.bounds.min.y ? SectionA : SectionB;
            section_b = section_a == SectionA ? SectionB : SectionA;
        }
    }
}
