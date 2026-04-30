# Manual FBX Import via Unity_RunCommand

Use this fallback when `Unity_ImportExternalModel` is unavailable and the user cannot or does not want to enable it via Project Settings.

Use `Unity_RunCommand` to replicate the import pipeline in C# code. The reference implementation lives in the `com.unity.ai.assistant` package:

**Package:** `com.unity.ai.assistant`
**Path within package:** `Modules/Unity.AI.MCP.Editor/Tools/ImportExternalModel.cs`

Read this file before implementing. The key method is `DownloadAndImportModelInScene()` which orchestrates the full pipeline. Replicate its logic across separate `Unity_RunCommand` calls so failures are isolated and debuggable.

## Step 1: Validate the FBX URL

Reference: `ValidateFilesURL()`, `ValidateFileExt()`, `IsHttpString()`, `IsInProject()`

Validate the file extension against supported types (`.fbx`, `.zip` for models; `.png`, `.jpg`, `.jpeg` for textures). Strip query parameters before checking extension using `ExtractFilenameFromUrl()` logic. Reject files already inside the project (those should use `Unity.ManageAsset` instead). Verify remote URLs start with `http://` or `https://`, and local paths resolve to existing files.

## Step 2: Download the asset to the project

Reference: `DownloadAsset()`, `HandleZipDownloadAndExtraction()`

Create a destination folder under `Assets/ExternalModels/{name}` using deduped naming (see `GetDedupedFolderName()`). Download via `UnityWebRequest.Get()` for both remote and local URLs. For `.zip` files, extract to a temp directory, locate the `.fbx` inside, and copy it plus any adjacent texture files to the destination. Call `AssetDatabase.Refresh()` after writing files.

## Step 3: Create or extract materials from the FBX

Reference: `ApplyFbxAssetImportSettings()`, `CreateOrExtractMaterialFromFBX()`, `ExtractMaterials()`

Apply import settings via `ModelImporter`: read the asset's `localScale` and set `globalScale` accordingly. If no albedo texture URL was provided, extract textures from the FBX using `importer.ExtractTextures()`. If an albedo texture URL was provided, download it separately, then extract materials from the FBX using `AssetDatabase.ExtractAsset()`. If no materials exist in the FBX, create a new Standard material. Apply the albedo texture to the material's `_MainTex` property.

## Step 4: Import model in scene with correct orientation and scale

Reference: `ImportModelInScene()`

Instantiate the FBX asset with `Object.Instantiate()`. Apply the material to the `MeshRenderer` if one was created. Add a `BoxCollider` to get accurate bounds, then scale the object so its height matches the desired height using `collider.bounds.size.y`. Call `Physics.SyncTransforms()` to update collider bounds after scaling. Reposition the object so its bottom sits at Y=0 (ground level) by calculating the bottom extent from `collider.bounds`.

## Step 5: Save as prefab

Reference: `SaveAsPrefab()`

Save the configured GameObject as a prefab in the destination folder using `PrefabUtility.SaveAsPrefabAssetAndConnect()`. Call `AssetDatabase.Refresh()` after saving.

## Execution notes

Execute the five steps **sequentially via separate `Unity_RunCommand` calls**. Each script must follow the `CommandScript : IRunCommand` pattern required by `Unity_RunCommand`.

If any step fails, stop and report the error. Clean up the destination folder on failure (as the reference implementation does).

After Step 5, report the same information as a successful `Unity_ImportExternalModel` call: import directory, GameObject data (name, instance ID, bounds), prefab path, and prefab data.
