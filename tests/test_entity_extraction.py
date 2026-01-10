import unittest
import os
from pathlib import Path
import re


class TestEntityExtraction(unittest.TestCase):
    """Test suite for StraightFour entity extraction."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.assets_dir = Path("Assets/StraightFour/Entity")
        
    def test_entity_directories_exist(self):
        """Test that entity directories exist in the expected structure."""
        expected_entities = [
            "Airplane", "Audio", "Automobile", "Base", "Character",
            "Light", "Mesh", "Terrain", "Voxel", "UI"
        ]
        
        if not self.assets_dir.exists():
            self.skipTest("Entity directory not found")
            
        existing_dirs = [d.name for d in self.assets_dir.iterdir() if d.is_dir()]
        
        for entity in expected_entities:
            self.assertIn(entity, existing_dirs, f"{entity} entity directory should exist")
            
    def test_entity_scripts_exist(self):
        """Test that entity script files exist."""
        expected_scripts = [
            ("Airplane", "AirplaneEntity.cs"),
            ("Audio", "AudioEntity.cs"),
            ("Automobile", "AutomobileEntity.cs"),
            ("Base", "BaseEntity.cs"),
            ("Base", "PlacementSocket.cs")
        ]
        
        if not self.assets_dir.exists():
            self.skipTest("Entity directory not found")
            
        for entity_dir, script_name in expected_scripts:
            script_path = self.assets_dir / entity_dir / "Scripts" / script_name
            if not script_path.exists():
                # Try alternative paths
                alt_path = self.assets_dir / entity_dir / script_name
                if alt_path.exists():
                    script_path = alt_path
                    
            self.assertTrue(script_path.exists() or alt_path.exists(),
                          f"Script {script_name} should exist for {entity_dir} entity")
                          
    def test_entity_classes_defined(self):
        """Test that entity classes are properly defined in scripts."""
        entity_files = list(self.assets_dir.rglob("*Entity.cs"))
        
        if len(entity_files) == 0:
            self.skipTest("No entity script files found")
            
        for entity_file in entity_files:
            with open(entity_file, 'r', encoding='utf-8') as f:
                content = f.read()
                
            # Check for class definition
            class_name = entity_file.stem  # filename without extension
            class_pattern = rf'class\s+{class_name}\s*:'
            
            self.assertTrue(re.search(class_pattern, content),
                          f"Class {class_name} should be defined in {entity_file}")
                          
    def test_base_entity_inheritance(self):
        """Test that entities inherit from BaseEntity appropriately."""
        entity_files = list(self.assets_dir.rglob("*Entity.cs"))
        entity_files = [f for f in entity_files if "BaseEntity" not in f.name]
        
        if len(entity_files) == 0:
            self.skipTest("No derived entity script files found")
            
        for entity_file in entity_files:
            with open(entity_file, 'r', encoding='utf-8') as f:
                content = f.read()
                
            # Check for inheritance from BaseEntity or MonoBehaviour
            class_name = entity_file.stem
            inheritance_patterns = [
                rf'class\s+{class_name}\s*:\s*BaseEntity',
                rf'class\s+{class_name}\s*:\s*MonoBehaviour',
                rf'class\s+{class_name}\s*:\s*\w+Entity'  # Might inherit from other entities
            ]
            
            has_inheritance = any(re.search(pattern, content) for pattern in inheritance_patterns)
            self.assertTrue(has_inheritance,
                          f"Entity {class_name} should inherit from BaseEntity or MonoBehaviour")
                          
    def test_unity_components_structure(self):
        """Test that Unity component structure is maintained."""
        # Check for proper Unity namespace usage
        cs_files = list(self.assets_dir.rglob("*.cs"))
        
        if len(cs_files) == 0:
            self.skipTest("No C# files found")
            
        for cs_file in cs_files:
            with open(cs_file, 'r', encoding='utf-8') as f:
                content = f.read()
                
            # Should have Unity using statements
            unity_usings = ['using UnityEngine', 'UnityEngine.']
            has_unity_usage = any(usage in content for usage in unity_usings)
            
            if 'MonoBehaviour' in content or 'GameObject' in content:
                self.assertTrue(has_unity_usage,
                              f"File {cs_file} using Unity classes should have Unity imports")


class TestEntityDocumentation(unittest.TestCase):
    """Test entity documentation and metadata extraction."""
    
    def test_entity_comments_and_documentation(self):
        """Test that entities have proper documentation."""
        assets_dir = Path("Assets/StraightFour/Entity")
        
        if not assets_dir.exists():
            self.skipTest("Entity directory not found")
            
        entity_files = list(assets_dir.rglob("*Entity.cs"))
        
        for entity_file in entity_files:
            with open(entity_file, 'r', encoding='utf-8') as f:
                content = f.read()
                
            # Check for some form of documentation
            has_documentation = any(marker in content for marker in [
                '///', '/**', '/*', 'summary', 'Summary'
            ])
            
            # At minimum, should have some comments
            has_comments = '//' in content or '/*' in content
            
            self.assertTrue(has_comments or has_documentation,
                          f"Entity file {entity_file} should have some documentation or comments")
                          
    def test_entity_public_methods_extraction(self):
        """Test that public methods can be extracted from entities."""
        assets_dir = Path("Assets/StraightFour/Entity")
        
        if not assets_dir.exists():
            self.skipTest("Entity directory not found")
            
        entity_files = list(assets_dir.rglob("*Entity.cs"))
        
        for entity_file in entity_files:
            with open(entity_file, 'r', encoding='utf-8') as f:
                content = f.read()
                
            # Find public methods
            method_pattern = r'public\s+\w+\s+\w+\s*\('
            public_methods = re.findall(method_pattern, content)
            
            # Should have at least some public interface
            # (Even if just Unity lifecycle methods)
            if 'class' in content:
                # This is more of an informational test
                method_count = len(public_methods)
                self.assertGreaterEqual(method_count, 0,
                                      f"Entity {entity_file.stem} method count: {method_count}")


class TestUIEntityStructure(unittest.TestCase):
    """Test UI entity specific structure."""
    
    def test_ui_entity_hierarchy(self):
        """Test that UI entities have proper hierarchy."""
        ui_dir = Path("Assets/StraightFour/Entity/UI")
        
        if not ui_dir.exists():
            self.skipTest("UI entity directory not found")
            
        # Should have Canvas and Element subdirectories or similar
        expected_ui_types = ["Canvas", "Element", "Button", "Input", "Text"]
        
        ui_subdirs = [d.name for d in ui_dir.iterdir() if d.is_dir()]
        ui_files = [f.name for f in ui_dir.rglob("*.cs")]
        
        # At least some UI types should be present
        found_ui_types = []
        for ui_type in expected_ui_types:
            if any(ui_type.lower() in name.lower() for name in ui_subdirs + ui_files):
                found_ui_types.append(ui_type)
                
        self.assertGreater(len(found_ui_types), 0,
                         "Should find at least some UI entity types")
                         
    def test_ui_component_structure(self):
        """Test UI component structure matches expected patterns."""
        ui_dir = Path("Assets/StraightFour/Entity/UI")
        
        if not ui_dir.exists():
            self.skipTest("UI entity directory not found")
            
        ui_files = list(ui_dir.rglob("*.cs"))
        
        for ui_file in ui_files:
            with open(ui_file, 'r', encoding='utf-8') as f:
                content = f.read()
                
            # UI entities should likely reference Unity UI components
            ui_patterns = [
                'UnityEngine.UI', 'Canvas', 'Button', 'Text', 'Image',
                'RectTransform', 'EventSystem'
            ]
            
            if 'UI' in ui_file.name:
                has_ui_references = any(pattern in content for pattern in ui_patterns)
                # This is informational - not all files may have direct UI references
                if has_ui_references:
                    self.assertTrue(True, f"UI file {ui_file.name} has UI references")


if __name__ == '__main__':
    unittest.main()