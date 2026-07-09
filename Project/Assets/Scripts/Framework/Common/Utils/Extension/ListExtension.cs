using System.Collections.Generic;

public static class ListExtension
{
    public static TParent[] ConvertToBaseArray<TChild, TParent>(this List<TChild> list) where TChild : TParent
    {
        TParent[] res = new TParent[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            res[i] = list[i];
        }

        return res;
    }
}