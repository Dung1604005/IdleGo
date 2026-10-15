using System.Collections.Generic;

public sealed class MapProgressViewRegistry
{
    private readonly List<IMapProgressView> views =
        new List<IMapProgressView>();

    public void Register(IMapProgressView view)
    {
        if (view != null && !views.Contains(view)) views.Add(view);
    }

    public void Unregister(IMapProgressView view)
    {
        if (view != null) views.Remove(view);
    }

    public void RefreshViews()
    {
        for (int i = views.Count - 1; i >= 0; i--)
        {
            IMapProgressView view = views[i];
            if (view == null)
            {
                views.RemoveAt(i);
                continue;
            }
            view.RefreshMapProgress();
        }
    }

    public void Clear()
    {
        views.Clear();
    }
}
