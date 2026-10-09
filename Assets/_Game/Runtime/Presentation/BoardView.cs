using System.Collections.Generic;
using UnityEngine;

namespace MathPirates.Presentation
{
    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<Material> materials = new();
        private readonly Dictionary<PrimitiveType, Mesh> meshes = new();
        private readonly Transform[] ships = new Transform[4];
        private readonly BoxCollider[] shipTargets = new BoxCollider[4];
        private readonly Vector3[] positions = new Vector3[4];
        private Mesh sailMesh;
        private Mesh hullMesh, deckMesh, waterMesh;
        public const float FeedbackDuration = 1.35f;
        private const float FlightDuration = .5f;
        private readonly Transform[] cannons = new Transform[8];
        private readonly Transform[] barrels = new Transform[8];
        private readonly Transform[] muzzles = new Transform[8];
        private readonly Transform[] debris = new Transform[6];
        private Transform cannonball, muzzleFlash, impact;
        private Vector3 launchPosition;
        private float feedbackTime;
        private int firingShip = -1;
        private bool correctShot;
        private Transform island;

        private bool ProjectileInFlight => firingShip >= 0 && feedbackTime < FlightDuration;
        private bool HasImpact => firingShip >= 0 && feedbackTime >= FlightDuration;

        public Vector3 LabelPosition(int index) => ships[index].TransformPoint(new Vector3(0, .69f, -.14f));

        public void Build(Shader shader, Shader waterShader)
        {
            var sea = new Material(waterShader) { name = "Transparent faceted waves" };
            sea.SetFloat("_Opacity", .4f);
            materials.Add(sea);
            var sand = Material(shader, "Sand", new Color(.98f, .80f, .43f));
            var grass = Material(shader, "Grass", new Color(.25f, .61f, .32f));
            var wood = Material(shader, "Wood", new Color(.30f, .16f, .13f));
            var deckWood = Material(shader, "Deck wood", new Color(.57f, .34f, .16f));
            var railWood = Material(shader, "Gunwale wood", new Color(.40f, .23f, .11f));
            var sail = Material(shader, "Sail", new Color(1f, .95f, .79f));
            var coral = Material(shader, "Coral", new Color(.95f, .30f, .24f));
            var stone = Material(shader, "Stone", new Color(.29f, .40f, .43f));
            var iron = Material(shader, "Cannon iron", new Color(.15f, .18f, .18f));
            var bore = Material(shader, "Cannon bore", new Color(.035f, .045f, .04f));
            var flash = Material(shader, "Cannon flash", new Color(1f, .62f, .14f));
            waterMesh = NavalMeshes.Water();
            hullMesh = NavalMeshes.Hull();
            deckMesh = NavalMeshes.Deck();
            var ocean = MeshObject(transform, "Waves · 60% transparent", waterMesh, sea);
            ocean.localPosition = new Vector3(0, -.09f, 0);
            island = new GameObject("Island").transform;
            island.SetParent(transform, false);
            Shape(PrimitiveType.Sphere, island, "Beach", Vector3.zero, new Vector3(2.3f, .45f, 1.8f), sand);
            Shape(PrimitiveType.Sphere, island, "Meadow", new Vector3(0, .12f, 0), new Vector3(1.75f, .32f, 1.3f), grass);
            for (int i = 0; i < 3; i++)
            {
                var palm = new GameObject("Palm").transform;
                palm.SetParent(island, false);
                palm.localPosition = new Vector3(-.55f + i * .52f, .2f, .25f + (i % 2) * .2f);
                palm.localScale = Vector3.one * (i == 1 ? 1.15f : .8f);
                Shape(PrimitiveType.Cylinder, palm, "Trunk", new Vector3(0, .48f, 0), new Vector3(.10f, .48f, .10f), wood);
                for (int leaf = 0; leaf < 5; leaf++)
                {
                    float angle = leaf * Mathf.PI * 2 / 5;
                    var frond = Shape(PrimitiveType.Sphere, palm, "Leaf", new Vector3(Mathf.Cos(angle) * .20f, .96f, Mathf.Sin(angle) * .20f), new Vector3(.65f, .07f, .19f), grass);
                    frond.localRotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 12);
                }
            }
            Shape(PrimitiveType.Cube, island, "Treasure", new Vector3(.2f, .33f, -.38f), new Vector3(.38f, .23f, .25f), wood);
            Shape(PrimitiveType.Cube, island, "Treasure band", new Vector3(.2f, .34f, -.38f), new Vector3(.07f, .25f, .27f), sand);
            for (int i = 0; i < 4; i++)
                Shape(PrimitiveType.Sphere, island, "Rock", new Vector3(-.85f + i * .14f, .13f, -.3f), new Vector3(.2f, .25f, .23f), stone);

            sailMesh = NavalMeshes.Sail();
            for (int i = 0; i < 4; i++)
            {
                var ship = new GameObject($"Answer ship {i + 1}").transform;
                ship.SetParent(transform, false);
                positions[i] = new Vector3(i % 2 == 0 ? -1.85f : 1.85f, 0, i < 2 ? .85f : -1.0f);
                ship.localPosition = positions[i];
                ship.localRotation = Quaternion.Euler(0, i % 2 == 0 ? -18 : 18, 0);
                ships[i] = ship;
                shipTargets[i] = ship.gameObject.AddComponent<BoxCollider>();
                shipTargets[i].center = new Vector3(0, .45f, 0);
                shipTargets[i].size = new Vector3(1.45f, 1.4f, .75f);
                MeshObject(ship, "Hull", hullMesh, wood);
                MeshObject(ship, "Recessed deck", deckMesh, deckWood);
                Vector3[] rail = { new(-.62f,.22f,.18f), new(-.43f,.16f,.29f), new(.27f,.18f,.25f), new(.65f,.30f,.015f) };
                for (int side = -1; side <= 1; side += 2)
                    for (int segment = 0; segment < rail.Length - 1; segment++)
                    {
                        var a = rail[segment]; var b = rail[segment + 1]; a.z *= side; b.z *= side;
                        var beam = Shape(PrimitiveType.Cube, ship, "Raised gunwale", (a + b) * .5f, new Vector3((b-a).magnitude, .045f, .045f), railWood);
                        beam.localRotation = Quaternion.FromToRotation(Vector3.right, b - a);
                    }
                Shape(PrimitiveType.Cube, ship, "Stern rail", new Vector3(-.62f,.22f,0), new Vector3(.045f,.045f,.38f), railWood);
                Shape(PrimitiveType.Cube, ship, "Stern cabin", new Vector3(-.42f,.25f,0), new Vector3(.20f,.23f,.30f), wood);
                Shape(PrimitiveType.Cube, ship, "Cabin roof", new Vector3(-.42f,.38f,0), new Vector3(.24f,.045f,.34f), deckWood);
                for (int plank = 0; plank < 3; plank++)
                    Shape(PrimitiveType.Cube, ship, "Deck seam", new Vector3(-.06f,.134f,-.13f + plank*.13f), new Vector3(.75f,.006f,.009f), railWood);
                Shape(PrimitiveType.Cylinder, ship, "Mast", new Vector3(0, .58f, 0), new Vector3(.045f, .51f, .045f), wood);
                var canvas = new GameObject("Sail", typeof(MeshFilter), typeof(MeshRenderer));
                canvas.transform.SetParent(ship, false);
                canvas.transform.localPosition = Vector3.zero;
                canvas.GetComponent<MeshFilter>().sharedMesh = sailMesh;
                canvas.GetComponent<MeshRenderer>().sharedMaterial = sail;
                Shape(PrimitiveType.Cube, ship, "Pennant", new Vector3(.14f, 1.1f, 0), new Vector3(.28f, .13f, .02f), coral);
                var direction = new Vector3(positions[i].x, 0, positions[i].z).normalized;
                CreateCannon(i, island, direction * .82f + Vector3.up * .27f, iron, bore, wood);
                CreateCannon(i + 4, ship, new Vector3(.30f, .25f, 0), iron, bore, wood);
            }
            cannonball = Shape(PrimitiveType.Sphere, transform, "Cannonball", Vector3.zero, Vector3.one * .12f, iron);
            muzzleFlash = Shape(PrimitiveType.Sphere, transform, "Muzzle flash", Vector3.zero, Vector3.one * .2f, flash);
            impact = Shape(PrimitiveType.Sphere, transform, "Impact flash", Vector3.zero, Vector3.one * .3f, flash);
            for (int i = 0; i < debris.Length; i++)
                debris[i] = Shape(PrimitiveType.Cube, transform, "Impact chip", Vector3.zero, new Vector3(.06f, .04f, .09f), deckWood);
            AimCannons();
            ResetFeedback();
        }

        public void Feedback(int index, bool correct)
        {
            firingShip = index;
            correctShot = correct;
            feedbackTime = 0;
            AimCannons();
            launchPosition = transform.InverseTransformPoint(muzzles[correct ? index : index + 4].position);
            cannonball.localPosition = launchPosition;
            cannonball.gameObject.SetActive(true);
            muzzleFlash.localPosition = launchPosition;
            muzzleFlash.gameObject.SetActive(true);
        }

        public bool TryPickShip(Ray ray, out int index)
        {
            index = -1;
            float nearest = float.PositiveInfinity;
            Physics.SyncTransforms();
            for (int i = 0; i < shipTargets.Length; i++)
                if (shipTargets[i] != null && shipTargets[i].gameObject.activeInHierarchy && shipTargets[i].Raycast(ray, out var hit, 100) && hit.distance < nearest)
                {
                    nearest = hit.distance;
                    index = i;
                }
            return index >= 0;
        }

        public void ResetFeedback()
        {
            feedbackTime = 0;
            firingShip = -1;
            if (cannonball != null) cannonball.gameObject.SetActive(false);
            if (muzzleFlash != null) muzzleFlash.gameObject.SetActive(false);
            if (impact != null) impact.gameObject.SetActive(false);
            foreach (var chip in debris) if (chip != null) chip.gameObject.SetActive(false);
            foreach (var barrel in barrels) if (barrel != null) barrel.localPosition = new Vector3(0, .06f, .1f);
            if (island != null) island.localPosition = Vector3.zero;
        }

        private void Update()
        {
            if (firingShip >= 0) feedbackTime += Time.deltaTime;
            float afterHit = Mathf.Max(0, feedbackTime - FlightDuration);
            for (int i = 0; i < ships.Length; i++)
            {
                if (ships[i] == null) continue;
                var position = positions[i];
                position.y += NavalMeshes.WaveHeight(position.x, position.z, Time.time) - .025f;
                if (HasImpact && correctShot && firingShip == i) position.y -= Mathf.Clamp01(afterHit / .75f) * 1.25f;
                ships[i].localPosition = position;
                float tilt = HasImpact && correctShot && firingShip == i ? Mathf.Clamp01(afterHit / .75f) * 18 : 0;
                ships[i].localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.1f + i) * 2.5f, i % 2 == 0 ? -18 : 18, Mathf.Sin(Time.time * .8f + i) * 2 + tilt);
            }
            if (island != null) island.localPosition = HasImpact && !correctShot ? new Vector3(Mathf.Sin(afterHit * 45) * .045f * (1 - Mathf.Clamp01(afterHit / .7f)), 0, 0) : Vector3.zero;
            AimCannons();
            AnimateShot(afterHit);
        }

        private void CreateCannon(int index, Transform parent, Vector3 position, Material iron, Material bore, Material wood)
        {
            var cannon = new GameObject(index < 4 ? $"Island cannon {index + 1}" : "Ship cannon").transform;
            cannon.SetParent(parent, false);
            cannon.localPosition = position;
            cannons[index] = cannon;
            Shape(PrimitiveType.Cube, cannon, "Carriage", new Vector3(0, -.025f, 0), new Vector3(.22f, .09f, .28f), wood);
            for (int side = -1; side <= 1; side += 2)
            {
                var wheel = Shape(PrimitiveType.Cylinder, cannon, "Wheel", new Vector3(side * .13f, -.03f, -.04f), new Vector3(.13f, .025f, .13f), iron);
                wheel.localRotation = Quaternion.Euler(0, 0, 90);
            }
            var barrel = new GameObject("Recoiling barrel").transform;
            barrel.SetParent(cannon, false);
            barrel.localPosition = new Vector3(0, .06f, .1f);
            barrels[index] = barrel;
            var tube = Shape(PrimitiveType.Cylinder, barrel, "Barrel", Vector3.zero, new Vector3(.13f, .17f, .13f), iron);
            tube.localRotation = Quaternion.Euler(90, 0, 0);
            var opening = Shape(PrimitiveType.Cylinder, barrel, "Muzzle opening", new Vector3(0, 0, .171f), new Vector3(.088f, .002f, .088f), bore);
            opening.localRotation = Quaternion.Euler(90, 0, 0);
            muzzles[index] = opening;
        }

        private void AimCannons()
        {
            for (int i = 0; i < 4; i++)
            {
                if (cannons[i] == null) continue;
                cannons[i].rotation = Quaternion.LookRotation(ships[i].TransformPoint(new Vector3(.15f, .25f, 0)) - cannons[i].position, transform.up);
                cannons[i + 4].rotation = Quaternion.LookRotation(island.TransformPoint(Vector3.up * .25f) - cannons[i + 4].position, transform.up);
            }
        }

        private void AnimateShot(float afterHit)
        {
            if (firingShip < 0) return;
            int source = correctShot ? firingShip : firingShip + 4;
            float recoil = feedbackTime < .22f ? Mathf.Sin(feedbackTime / .22f * Mathf.PI) * .065f : 0;
            barrels[source].localPosition = new Vector3(0, .06f, .1f - recoil);
            muzzleFlash.gameObject.SetActive(feedbackTime < .12f);
            muzzleFlash.localScale = Vector3.one * (.22f * (1 - Mathf.Clamp01(feedbackTime / .12f)));
            var target = correctShot ? positions[firingShip] + Vector3.up * .25f : positions[firingShip].normalized * .5f + Vector3.up * .25f;
            cannonball.gameObject.SetActive(ProjectileInFlight);
            if (ProjectileInFlight)
            {
                float t = Mathf.Clamp01(feedbackTime / FlightDuration);
                cannonball.localPosition = Vector3.Lerp(launchPosition, target, t) + Vector3.up * (4 * t * (1 - t) * .45f);
            }
            impact.gameObject.SetActive(HasImpact && afterHit < .2f);
            impact.localPosition = target;
            impact.localScale = Vector3.one * Mathf.Lerp(.12f, .48f, Mathf.Clamp01(afterHit / .2f));
            for (int i = 0; i < debris.Length; i++)
            {
                debris[i].gameObject.SetActive(HasImpact && afterHit < .55f);
                float angle = i * Mathf.PI * 2 / debris.Length;
                debris[i].localPosition = target + new Vector3(Mathf.Cos(angle) * afterHit, afterHit * 1.6f - afterHit * afterHit * 4, Mathf.Sin(angle) * afterHit);
                debris[i].localRotation = Quaternion.Euler(afterHit * 360, i * 60, afterHit * 240);
            }
        }

        private Material Material(Shader shader, string label, Color color)
        {
            var material = new Material(shader) { name = label, color = color };
            material.SetFloat("_Smoothness", .12f);
            materials.Add(material);
            return material;
        }

        private Transform Shape(PrimitiveType type, Transform parent, string label, Vector3 position, Vector3 scale, Material material)
        {
            if (!meshes.TryGetValue(type, out var mesh)) { mesh = LowPolyMesh.Create(type); meshes.Add(type, mesh); }
            var shape = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            shape.name = label;
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = position;
            shape.transform.localScale = scale;
            shape.GetComponent<MeshFilter>().sharedMesh = mesh;
            shape.GetComponent<MeshRenderer>().sharedMaterial = material;
            return shape.transform;
        }

        private static Transform MeshObject(Transform parent, string label, Mesh mesh, Material material)
        {
            var item = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(parent, false);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = material;
            return item.transform;
        }

        private void OnDestroy()
        {
            foreach (var material in materials) Destroy(material);
            foreach (var mesh in meshes.Values) Destroy(mesh);
            if (sailMesh != null) Destroy(sailMesh);
            if (hullMesh != null) Destroy(hullMesh);
            if (deckMesh != null) Destroy(deckMesh);
            if (waterMesh != null) Destroy(waterMesh);
        }
    }
}
