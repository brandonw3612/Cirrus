using Impressionist.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Impressionist;
public interface IColorSelector
{
    public List<ArgbColor> SelectColors(Dictionary<ArgbColor, int> colorsToPopulation, int desired = 4);
}
