# Project Organizer Tool

A modern Unity Editor tool to **organize and migrate your project files** into a clean, professional folder structure — safely, visually, and with full undo support.

---

## How to Install

This tool is provided as a **Unity package**.

1. In Unity, open the menu: **Window > Package Manager**.
2. Click the **+** button (top left) and select **Add package from disk...**
3. Select the `package.json` file located at the root of the ProjectOrganizerTool package.
4. Once imported, access the tool via `Tools > 🗂️ Project Organizer` in the Unity editor.

---

## Usage

1. **Set Up Rules and Folders**

   - By default, the tool scans the `Assets` folder.
   - You can add or remove included/excluded folders in the config panel.
   - Set extension mappings to define where each file type should go.

2. **Set File Types to Scan**

   - Enter the file extensions you want to manage (comma or semicolon separated).
   - Example: `.cs, .shader, .png`

3. **Exclude Folders**

   - System folders (`ProjectSettings`, `Packages`, etc.) are auto-excluded.
   - Add your own folders to exclude as needed.

4. **Scan and Select Files**

   - Click **Scan files** to find all files matching your settings.
   - Use search and toggles to select which files you want to move.

5. **Preview Moves & Dry Run**

   - See exactly what will be moved, from where, to where.
   - Use **Dry run** to simulate the move without making any changes.

6. **Organize!**

   - Hit **🚀 ORGANIZE NOW** to actually move all selected files.
   - The folder structure is auto-created as needed.
   - A log appears at the bottom with a summary.

7. **Undo Last Move**

   - You can **undo the last organize** operation using the **UNDO** button (as long as you haven’t closed Unity).
   - For extra safety, always use version control.

---

## Folder Structure (Default Template)

```text
Assets/
  Art/
    Sprites/
    Textures/
    Animations/
    Characters/
    Environment/
    UI/
  Audio/
    Music/
    SFX/
    Voices/
  Materials/
    Characters/
    Environment/
  Prefabs/
    Characters/
    Enemies/
    Environment/
    Props/
    UI/
  Scenes/
    Main/
    Levels/
    UI/
  Scripts/
    Core/
    Managers/
    Gameplay/
    Player/
    Enemies/
    UI/
    Utils/
  Shaders/
  UI/
    Images/
    Fonts/
  Plugins/
  Resources/
  Editor/
  StreamingAssets/
  Video/
  Misc/

---

## FAQ

Q: Does it delete files?
A: No. Files are only moved using Unity’s AssetDatabase.MoveAsset. No files are deleted.

Q: Can I undo?
A: Use version control! Unity’s undo system does not support asset moves like this. It is highly recommended to use Git or Plastic SCM.

Q: Will it move files outside Assets?
A: No, only files inside `Assets` (and subfolders) are moved.

---

## Support

For questions, suggestions, or bug reports, please contact me directly at jules.gilli@live.fr.
---

## License

MIT License

---

Project Organizer Tool © 2025
