using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    static class TakeRecorderContextExtensions
    {
        static void Validate(ITakeRecorderContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
        }

        public static bool HasSelection(this ITakeRecorderContext ctx)
        {
            Validate(ctx);

            return ctx.IsIndexValid(ctx.Selection);
        }

        public static bool IsIndexValid(this ITakeRecorderContext ctx, int index)
        {
            Validate(ctx);

            return index >= 0 && index < ctx.Shots.Length;
        }

        public static Shot? GetSelectedShot(this ITakeRecorderContext ctx)
        {
            Validate(ctx);

            if (ctx.HasSelection())
            {
                return ctx.Shots[ctx.Selection];
            }

            return null;
        }

        public static void SelectIndex(this ITakeRecorderContext ctx, int index)
        {
            Validate(ctx);

            ctx.Selection = Mathf.Clamp(index, -1, ctx.Shots.Length - 1);
        }

        public static void ClearSelection(this ITakeRecorderContext ctx)
        {
            Validate(ctx);

            ctx.SelectIndex(-1);
        }

        public static void SetShotAndBindings(this ITakeRecorderContext ctx, int index, in Shot shot)
        {
            Validate(ctx);

            if (ctx.IsIndexValid(index))
            {
                var oldShot = ctx.Shots[index];
                var requiresReloadBindings = oldShot.Take != shot.Take
                    || oldShot.IterationBase != shot.IterationBase;

                if (requiresReloadBindings)
                {
                    ctx.ClearSceneBindings(index);
                }

                ctx.SetShot(index, shot);

                if (requiresReloadBindings)
                {
                    ctx.SetSceneBindings(index);
                }
            }
        }

        public static void SetShotAndBindings(this ITakeRecorderContext ctx, in Shot shot)
        {
            ctx.SetShotAndBindings(ctx.Selection, in shot);
        }

        public static IExposedPropertyTable GetResolver(this ITakeRecorderContext ctx)
        {
            if (ctx.HasSelection())
            {
                return ctx.GetResolver(ctx.Selection);
            }

            return null;
        }

        public static void ClearSceneBindings(this ITakeRecorderContext ctx)
        {
            if (ctx.HasSelection())
            {
                ctx.ClearSceneBindings(ctx.Selection);
            }
        }

        public static void SetSceneBindings(this ITakeRecorderContext ctx)
        {
            if (ctx.HasSelection())
            {
                ctx.SetSceneBindings(ctx.Selection);
            }
        }

        public static void Rebuild(this ITakeRecorderContext ctx)
        {
            if (ctx.HasSelection())
            {
                ctx.Rebuild(ctx.Selection);
            }
        }

        public static void SetTake(this ITakeRecorderContext ctx, Take take)
        {
            Validate(ctx);

            if (ctx.GetSelectedShot() is Shot shot)
            {
                shot.Take = take;

                ctx.SetShotAndBindings(shot);
            }
        }

        public static void SetIterationBase(this ITakeRecorderContext ctx, Take take)
        {
            Validate(ctx);

            if (ctx.GetSelectedShot() is Shot shot)
            {
                shot.IterationBase = take;

                ctx.SetShotAndBindings(shot);
            }
        }

        public static UnityEngine.Object GetShotStorage(this ITakeRecorderContext ctx)
        {
            if (ctx.HasSelection())
            {
                return ctx.GetStorage(ctx.Selection);
            }

            return null;
        }
    }
}
