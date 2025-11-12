#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Unity UI Files Rename Script
Renames UI*.cs files to Crystal*.cs
"""

import os
import sys

# Target directory
UI_DIR = r"D:\GitProjects\CystalUnbolt\Assets\CrystalUnbolt\CrystalCrystalUnbolt/CrystalUnBoltGame\Game\Scripts\UI"

# Files to rename (old_name -> new_name)
FILES_TO_RENAME = {
    "UIComplete.cs": "CrystalUIComplete.cs",
    "UIDailySpin.cs": "CrystalUIDailySpin.cs",
    "UIGame.cs": "CrystalUIGame.cs",
    "UIGameOver.cs": "CrystalUIGameOver.cs",
    "UILeaderBoard.cs": "CrystalUILeaderBoard.cs",
    "UIMainMenu.cs": "CrystalUIMainMenu.cs",
    "UIMainMenuButton.cs": "CrystalUIMainMenuButton.cs",
    "UIPause.cs": "CrystalUIPause.cs",
    "UIProfilePage.cs": "CrystalUIProfilePage.cs",
    "UIStore.cs": "CrystalUIStore.cs",
}

def main():
    print("=" * 50)
    print("Unity UI Files Rename Script")
    print("=" * 50)
    print()
    
    # Check if directory exists
    if not os.path.exists(UI_DIR):
        print(f"ERROR: Directory not found: {UI_DIR}")
        input("Press Enter to exit...")
        sys.exit(1)
    
    print(f"Working directory: {UI_DIR}")
    print()
    
    # Change to target directory
    os.chdir(UI_DIR)
    
    success_count = 0
    skip_count = 0
    error_count = 0
    
    # Rename .cs files
    for old_name, new_name in FILES_TO_RENAME.items():
        old_cs = old_name
        new_cs = new_name
        old_meta = old_name + ".meta"
        new_meta = new_name + ".meta"
        
        try:
            if os.path.exists(old_cs):
                # Rename .cs file
                os.rename(old_cs, new_cs)
                print(f"✓ {old_cs} -> {new_cs}")
                
                # Rename .meta file
                if os.path.exists(old_meta):
                    os.rename(old_meta, new_meta)
                    print(f"✓ {old_meta} -> {new_meta}")
                
                success_count += 1
            else:
                print(f"⊘ {old_cs} (already renamed or not found)")
                skip_count += 1
                
        except Exception as e:
            print(f"✗ ERROR renaming {old_cs}: {e}")
            error_count += 1
    
    print()
    print("=" * 50)
    print("SUMMARY:")
    print(f"  ✓ Renamed: {success_count}")
    print(f"  ⊘ Skipped: {skip_count}")
    print(f"  ✗ Errors:  {error_count}")
    print("=" * 50)
    print()
    
    if success_count > 0:
        print("SUCCESS! All files renamed successfully!")
        print()
        print("Next steps:")
        print("1. Open Unity")
        print("2. Wait for compile (1-2 minutes)")
        print("3. Check Console - should be 0 errors!")
    else:
        print("No files were renamed. They may already be renamed.")
    
    print()
    input("Press Enter to exit...")

if __name__ == "__main__":
    main()





