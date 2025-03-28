﻿using System.Collections.ObjectModel;

namespace Swashbuckle.AspNetCore.TestSupport;

public class KeyedCollectionOfComplexType : KeyedCollection<int, ComplexType>
{
    protected override int GetKeyForItem(ComplexType item) => item.Property2;
}
