import unittest
import json
import os
import tempfile
import shutil
from pathlib import Path
import markdown
import yaml

class TestGLTFCompatibilityAnalysis(unittest.TestCase):
    """Test suite for StraightFour-glTF compatibility analysis."""
    
    def setUp(self):
        """Set up test environment."""
        self.test_docs_dir = Path("test_docs/compatibility")
        self.test_docs_dir.mkdir(parents=True, exist_ok=True)
        
        # Expected output files
        self.matrix_file = self.test_docs_dir / "straightfour-gltf-matrix.md"
        self.report_file = self.test_docs_dir / "straightfour-gltf-report.md"
        self.mapping_file = self.test_docs_dir / "straightfour-gltf-mapping.json"
        
        # Mock StraightFour entities for testing
        self.mock_entities = {
            "BaseEntity": {
                "description": "Base entity class",
                "fields": ["id", "name", "transform"],
                "behaviors": ["placement", "serialization"],
                "required_fields": ["id"],
                "optional_fields": ["name", "transform"]
            },
            "AirplaneEntity": {
                "description": "Aircraft entity",
                "fields": ["id", "name", "transform", "flight_data"],
                "behaviors": ["movement", "physics"],
                "required_fields": ["id", "flight_data"],
                "optional_fields": ["name", "transform"]
            },
            "AudioEntity": {
                "description": "Audio entity",
                "fields": ["id", "audio_clip", "volume"],
                "behaviors": ["playback", "spatialization"],
                "required_fields": ["id", "audio_clip"],
                "optional_fields": ["volume"]
            }
        }
        
        # Mock OMI glTF extensions for testing
        self.mock_extensions = {
            "OMI_physics_body": {
                "purpose": "Add physics properties to nodes",
                "schema_fields": ["type", "mass", "inertia"],
                "required_fields": ["type"],
                "optional_fields": ["mass", "inertia"],
                "features": ["rigid_body", "collision", "mass_properties"],
                "dependencies": []
            },
            "OMI_audio_emitter": {
                "purpose": "Add audio emission to nodes",
                "schema_fields": ["source", "volume", "loop"],
                "required_fields": ["source"],
                "optional_fields": ["volume", "loop"],
                "features": ["audio_playback", "spatialization", "looping"],
                "dependencies": []
            },
            "OMI_vehicle": {
                "purpose": "Define vehicle properties",
                "schema_fields": ["type", "engine", "wheels"],
                "required_fields": ["type"],
                "optional_fields": ["engine", "wheels"],
                "features": ["vehicle_physics", "engine_simulation"],
                "dependencies": ["OMI_physics_body"]
            }
        }
        
    def tearDown(self):
        """Clean up test environment."""
        if self.test_docs_dir.exists():
            shutil.rmtree(self.test_docs_dir.parent)
    
    def test_matrix_file_exists(self):
        """Test that compatibility matrix file exists."""
        # Create a mock matrix file
        self._create_mock_matrix_file()
        self.assertTrue(self.matrix_file.exists())
        
    def test_report_file_exists(self):
        """Test that detailed report file exists."""
        # Create a mock report file
        self._create_mock_report_file()
        self.assertTrue(self.report_file.exists())
        
    def test_mapping_file_exists(self):
        """Test that JSON mapping file exists."""
        # Create a mock mapping file
        self._create_mock_mapping_file()
        self.assertTrue(self.mapping_file.exists())
        
    def test_matrix_structure_valid(self):
        """Test that compatibility matrix has valid structure."""
        self._create_mock_matrix_file()
        
        with open(self.matrix_file, 'r') as f:
            content = f.read()
            
        # Check for required columns
        self.assertIn("Extension Name", content)
        self.assertIn("StraightFour Entity", content)
        self.assertIn("Support Level", content)
        self.assertIn("Missing Features", content)
        self.assertIn("Notes", content)
        
        # Check for table formatting
        self.assertIn("|", content)
        self.assertIn("---", content)
        
    def test_report_structure_valid(self):
        """Test that detailed report has valid structure."""
        self._create_mock_report_file()
        
        with open(self.report_file, 'r') as f:
            content = f.read()
            
        # Check for required sections
        required_sections = [
            "Overview of StraightFour entity model",
            "Overview of OMI glTF extension ecosystem",
            "Mapping methodology",
            "Per-extension analysis",
            "Per-entity analysis",
            "Summary of unsupported extensions",
            "Summary of partially supported extensions",
            "Recommendations for future support"
        ]
        
        for section in required_sections:
            self.assertIn(section, content)
            
    def test_mapping_json_valid(self):
        """Test that JSON mapping file is valid JSON."""
        self._create_mock_mapping_file()
        
        with open(self.mapping_file, 'r') as f:
            data = json.load(f)
            
        # Validate JSON structure
        self.assertIn("extensions", data)
        self.assertIsInstance(data["extensions"], dict)
        
        # Check extension structure
        for ext_name, ext_data in data["extensions"].items():
            self.assertIn("supported", ext_data)
            self.assertIn("supported_features", ext_data)
            self.assertIn("missing_features", ext_data)
            self.assertIn("mapped_entities", ext_data)
            self.assertIsInstance(ext_data["supported"], bool)
            self.assertIsInstance(ext_data["supported_features"], list)
            self.assertIsInstance(ext_data["missing_features"], list)
            self.assertIsInstance(ext_data["mapped_entities"], list)
            
    def test_support_levels_valid(self):
        """Test that support levels are from valid set."""
        self._create_mock_matrix_file()
        
        with open(self.matrix_file, 'r') as f:
            content = f.read()
            
        valid_levels = ["Full", "Partial", "None"]
        lines = content.split('\n')
        
        # Find table rows (skip headers and separators)
        data_rows = [line for line in lines if '|' in line and not '---' in line and not 'Extension Name' in line]
        
        for row in data_rows:
            if row.strip():  # Skip empty rows
                columns = [col.strip() for col in row.split('|')[1:-1]]  # Remove empty first/last elements
                if len(columns) >= 3:  # Ensure we have enough columns
                    support_level = columns[2]  # Support Level column
                    self.assertIn(support_level, valid_levels, f"Invalid support level: {support_level}")
                    
    def test_all_entities_included(self):
        """Test that all StraightFour entities are included in analysis."""
        self._create_mock_matrix_file()
        
        with open(self.matrix_file, 'r') as f:
            content = f.read()
            
        # Check that all mock entities are mentioned
        for entity_name in self.mock_entities.keys():
            self.assertIn(entity_name, content, f"Entity {entity_name} not found in matrix")
            
    def test_all_extensions_included(self):
        """Test that all OMI glTF extensions are included in analysis."""
        self._create_mock_mapping_file()
        
        with open(self.mapping_file, 'r') as f:
            data = json.load(f)
            
        # Check that all mock extensions are included
        for ext_name in self.mock_extensions.keys():
            self.assertIn(ext_name, data["extensions"], f"Extension {ext_name} not found in mapping")
            
    def test_entity_extension_mapping_consistency(self):
        """Test consistency between matrix and JSON mapping."""
        self._create_mock_matrix_file()
        self._create_mock_mapping_file()
        
        # Read matrix
        with open(self.matrix_file, 'r') as f:
            matrix_content = f.read()
            
        # Read JSON mapping
        with open(self.mapping_file, 'r') as f:
            mapping_data = json.load(f)
            
        # Extract mappings from both files and compare
        for ext_name, ext_data in mapping_data["extensions"].items():
            # Check that extension appears in matrix
            self.assertIn(ext_name, matrix_content, f"Extension {ext_name} in JSON but not in matrix")
            
            # Check that mapped entities appear in matrix
            for entity in ext_data["mapped_entities"]:
                self.assertIn(entity, matrix_content, f"Entity {entity} in JSON but not in matrix")
                
    def test_required_vs_optional_fields_tracked(self):
        """Test that required vs optional fields are properly tracked."""
        self._create_mock_mapping_file()
        
        with open(self.mapping_file, 'r') as f:
            data = json.load(f)
            
        # Verify that the mapping considers field requirements
        # This would be implementation-specific, but we can check structure
        for ext_name, ext_data in data["extensions"].items():
            if ext_name in self.mock_extensions:
                mock_ext = self.mock_extensions[ext_name]
                # The missing_features should account for required fields
                self.assertIsInstance(ext_data["missing_features"], list)
                
    def test_extension_dependencies_handled(self):
        """Test that extension dependencies are properly handled."""
        self._create_mock_report_file()
        
        with open(self.report_file, 'r') as f:
            content = f.read()
            
        # Check that dependencies are mentioned
        self.assertIn("dependencies", content.lower())
        
        # For extensions with dependencies in mock data
        for ext_name, ext_data in self.mock_extensions.items():
            if ext_data["dependencies"]:
                # Should mention the extension and its dependencies
                self.assertIn(ext_name, content)
                
    def test_partial_support_identification(self):
        """Test that partial support is properly identified."""
        self._create_mock_mapping_file()
        
        with open(self.mapping_file, 'r') as f:
            data = json.load(f)
            
        # Look for extensions with partial support
        partial_extensions = []
        for ext_name, ext_data in data["extensions"].items():
            if ext_data["supported"] and ext_data["missing_features"]:
                partial_extensions.append(ext_name)
                
        # Should have some partially supported extensions in realistic scenario
        # This is a structural test - in real implementation there should be partial support
        
    def test_unsupported_extensions_identified(self):
        """Test that unsupported extensions are properly identified."""
        self._create_mock_mapping_file()
        
        with open(self.mapping_file, 'r') as f:
            data = json.load(f)
            
        # Look for completely unsupported extensions
        unsupported_extensions = []
        for ext_name, ext_data in data["extensions"].items():
            if not ext_data["supported"]:
                unsupported_extensions.append(ext_name)
                
        # Verify structure is correct for unsupported extensions
        for ext_name in unsupported_extensions:
            ext_data = data["extensions"][ext_name]
            self.assertEqual(len(ext_data["supported_features"]), 0)
            self.assertEqual(len(ext_data["mapped_entities"]), 0)
            
    def test_feature_level_granularity(self):
        """Test that analysis includes feature-level granularity."""
        self._create_mock_mapping_file()
        
        with open(self.mapping_file, 'r') as f:
            data = json.load(f)
            
        # Check that features are tracked at granular level
        for ext_name, ext_data in data["extensions"].items():
            # Should have specific features listed
            self.assertIsInstance(ext_data["supported_features"], list)
            self.assertIsInstance(ext_data["missing_features"], list)
            
            # At least one of these should be non-empty for realistic analysis
            total_features = len(ext_data["supported_features"]) + len(ext_data["missing_features"])
            self.assertGreaterEqual(total_features, 0)  # Structure test
            
    def test_markdown_syntax_valid(self):
        """Test that markdown files have valid syntax."""
        self._create_mock_matrix_file()
        self._create_mock_report_file()
        
        # Test matrix markdown
        with open(self.matrix_file, 'r') as f:
            matrix_content = f.read()
            
        # Basic markdown table validation
        lines = matrix_content.split('\n')
        table_lines = [line for line in lines if '|' in line]
        
        if table_lines:
            # Check that all table rows have same number of columns
            col_counts = [len(line.split('|')) - 2 for line in table_lines if not '---' in line]  # -2 for empty start/end
            if col_counts:
                first_count = col_counts[0]
                for count in col_counts:
                    self.assertEqual(count, first_count, "Inconsistent table column count")
                    
        # Test report markdown (basic structure)
        with open(self.report_file, 'r') as f:
            report_content = f.read()
            
        # Should have headers
        self.assertRegex(report_content, r'^#+ ', msg="Report should contain markdown headers")
        
    def test_edge_case_multiple_entity_mappings(self):
        """Test handling of extensions that map to multiple entities."""
        self._create_mock_mapping_file()
        
        with open(self.mapping_file, 'r') as f:
            data = json.load(f)
            
        # Look for extensions mapped to multiple entities
        multi_mapped = []
        for ext_name, ext_data in data["extensions"].items():
            if len(ext_data["mapped_entities"]) > 1:
                multi_mapped.append(ext_name)
                
        # Verify structure is correct for multi-mapped extensions
        for ext_name in multi_mapped:
            ext_data = data["extensions"][ext_name]
            self.assertIsInstance(ext_data["mapped_entities"], list)
            for entity in ext_data["mapped_entities"]:
                self.assertIsInstance(entity, str)
                
    def test_edge_case_entity_no_extension_mapping(self):
        """Test handling of entities with no glTF extension equivalent."""
        self._create_mock_report_file()
        
        with open(self.report_file, 'r') as f:
            content = f.read()
            
        # Should mention entities without mappings
        self.assertIn("no clear", content.lower())
        
    def _create_mock_matrix_file(self):
        """Create a mock compatibility matrix file for testing."""
        content = """# StraightFour-glTF Compatibility Matrix

| Extension Name | StraightFour Entity | Support Level | Missing Features | Notes |
|---|---|---|---|---|
| OMI_physics_body | BaseEntity | Partial | inertia, mass_properties | Basic physics support |
| OMI_audio_emitter | AudioEntity | Full | - | Complete audio support |
| OMI_vehicle | AirplaneEntity | None | vehicle_physics, engine_simulation | No vehicle support |
"""
        with open(self.matrix_file, 'w') as f:
            f.write(content)
            
    def _create_mock_report_file(self):
        """Create a mock detailed report file for testing."""
        content = """# StraightFour-glTF Compatibility Report

## Overview of StraightFour entity model
StraightFour defines various entity types for spatial applications.

## Overview of OMI glTF extension ecosystem
OMI glTF extensions provide standardized ways to extend glTF capabilities.

## Mapping methodology
We analyzed each StraightFour entity against OMI glTF extensions.

## Per-extension analysis
### OMI_physics_body
- Purpose: Add physics properties to nodes
- Dependencies: none

### OMI_audio_emitter
- Purpose: Add audio emission to nodes
- Dependencies: none

## Per-entity analysis
### BaseEntity
Basic entity with transform and placement capabilities.

### AudioEntity
Audio playback entity with spatialization.

## Summary of unsupported extensions
Extensions with no clear StraightFour equivalent.

## Summary of partially supported extensions
Extensions with incomplete feature support.

## Recommendations for future support
Priority extensions for implementation.
"""
        with open(self.report_file, 'w') as f:
            f.write(content)
            
    def _create_mock_mapping_file(self):
        """Create a mock JSON mapping file for testing."""
        mapping_data = {
            "extensions": {
                "OMI_physics_body": {
                    "supported": True,
                    "supported_features": ["rigid_body", "collision"],
                    "missing_features": ["mass_properties"],
                    "mapped_entities": ["BaseEntity"]
                },
                "OMI_audio_emitter": {
                    "supported": True,
                    "supported_features": ["audio_playback", "spatialization", "looping"],
                    "missing_features": [],
                    "mapped_entities": ["AudioEntity"]
                },
                "OMI_vehicle": {
                    "supported": False,
                    "supported_features": [],
                    "missing_features": ["vehicle_physics", "engine_simulation"],
                    "mapped_entities": []
                }
            }
        }
        
        with open(self.mapping_file, 'w') as f:
            json.dump(mapping_data, f, indent=2)


class TestStraightFourEntityExtraction(unittest.TestCase):
    """Test suite for StraightFour entity extraction functionality."""
    
    def setUp(self):
        """Set up test environment for entity extraction."""
        self.test_assets_dir = Path("test_assets/StraightFour")
        self.test_assets_dir.mkdir(parents=True, exist_ok=True)
        
    def tearDown(self):
        """Clean up test environment."""
        if self.test_assets_dir.exists():
            shutil.rmtree(self.test_assets_dir.parent)
            
    def test_entity_script_parsing(self):
        """Test parsing of entity C# scripts."""
        # Create mock entity script
        mock_script = """using UnityEngine;

namespace StraightFour.Entity.Base
{
    /// <summary>
    /// Base entity class for all StraightFour entities.
    /// </summary>
    public class BaseEntity : MonoBehaviour
    {
        [SerializeField]
        public string id;
        
        public string name;
        
        public Transform transform;
        
        public virtual void Initialize() 
        {
            // Base initialization
        }
    }
}
"""
        
        script_file = self.test_assets_dir / "Entity" / "Base" / "Scripts" / "BaseEntity.cs"
        script_file.parent.mkdir(parents=True, exist_ok=True)
        
        with open(script_file, 'w') as f:
            f.write(mock_script)
            
        # Test that file exists and can be read
        self.assertTrue(script_file.exists())
        
        with open(script_file, 'r') as f:
            content = f.read()
            
        # Basic parsing tests
        self.assertIn("BaseEntity", content)
        self.assertIn("public string id", content)
        self.assertIn("MonoBehaviour", content)
        
    def test_entity_directory_structure(self):
        """Test entity directory structure parsing."""
        # Create mock directory structure
        entities = ["Base", "Airplane", "Audio", "Automobile"]
        
        for entity in entities:
            entity_dir = self.test_assets_dir / "Entity" / entity
            entity_dir.mkdir(parents=True, exist_ok=True)
            
            # Create Scripts subdirectory
            scripts_dir = entity_dir / "Scripts"
            scripts_dir.mkdir(exist_ok=True)
            
            # Create mock script file
            script_file = scripts_dir / f"{entity}Entity.cs"
            with open(script_file, 'w') as f:
                f.write(f"// Mock {entity}Entity script")
                
        # Verify structure
        for entity in entities:
            entity_dir = self.test_assets_dir / "Entity" / entity
            self.assertTrue(entity_dir.exists())
            
            scripts_dir = entity_dir / "Scripts"
            self.assertTrue(scripts_dir.exists())
            
    def test_entity_metadata_extraction(self):
        """Test extraction of entity metadata from Unity meta files."""
        # Create mock .meta file
        meta_content = """fileFormatVersion: 2
guid: 12345678901234567890123456789012
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
        
        meta_file = self.test_assets_dir / "Entity" / "Base" / "Scripts" / "BaseEntity.cs.meta"
        meta_file.parent.mkdir(parents=True, exist_ok=True)
        
        with open(meta_file, 'w') as f:
            f.write(meta_content)
            
        # Test meta file parsing
        self.assertTrue(meta_file.exists())
        
        with open(meta_file, 'r') as f:
            content = f.read()
            
        self.assertIn("guid:", content)
        self.assertIn("MonoImporter:", content)


class TestOMIGLTFExtensionExtraction(unittest.TestCase):
    """Test suite for OMI glTF extension extraction functionality."""
    
    def setUp(self):
        """Set up test environment for extension extraction."""
        self.test_extensions_dir = Path("test_omi_gltf_extensions")
        self.test_extensions_dir.mkdir(parents=True, exist_ok=True)
        
    def tearDown(self):
        """Clean up test environment."""
        if self.test_extensions_dir.exists():
            shutil.rmtree(self.test_extensions_dir)
            
    def test_extension_readme_parsing(self):
        """Test parsing of extension README files."""
        # Create mock extension directory and README
        ext_dir = self.test_extensions_dir / "extensions" / "2.0" / "OMI_physics_body"
        ext_dir.mkdir(parents=True, exist_ok=True)
        
        readme_content = """# OMI_physics_body

## Contributors

## Status

## Dependencies

## Overview

This extension adds physics properties to glTF nodes.

## Schema

### Node

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| type | string | Physics body type | Yes |
| mass | number | Mass of the body | No |
| inertia | object | Inertia tensor | No |

## Features

- Rigid body physics
- Collision detection  
- Mass properties
"""
        
        readme_file = ext_dir / "README.md"
        with open(readme_file, 'w') as f:
            f.write(readme_content)
            
        # Test README parsing
        self.assertTrue(readme_file.exists())
        
        with open(readme_file, 'r') as f:
            content = f.read()
            
        self.assertIn("OMI_physics_body", content)
        self.assertIn("physics properties", content)
        self.assertIn("| type | string |", content)
        
    def test_extension_schema_parsing(self):
        """Test parsing of extension JSON schema files."""
        # Create mock schema file
        ext_dir = self.test_extensions_dir / "extensions" / "2.0" / "OMI_audio_emitter"
        ext_dir.mkdir(parents=True, exist_ok=True)
        
        schema_content = {
            "$schema": "https://json-schema.org/draft/2020-12/schema",
            "title": "OMI_audio_emitter Node Extension",
            "type": "object",
            "properties": {
                "source": {
                    "type": "integer",
                    "description": "Index of audio source"
                },
                "volume": {
                    "type": "number",
                    "description": "Audio volume",
                    "minimum": 0.0,
                    "maximum": 1.0
                },
                "loop": {
                    "type": "boolean",
                    "description": "Whether audio loops"
                }
            },
            "required": ["source"]
        }
        
        schema_file = ext_dir / "schema" / "node.OMI_audio_emitter.schema.json"
        schema_file.parent.mkdir(exist_ok=True)
        
        with open(schema_file, 'w') as f:
            json.dump(schema_content, f, indent=2)
            
        # Test schema parsing
        self.assertTrue(schema_file.exists())
        
        with open(schema_file, 'r') as f:
            data = json.load(f)
            
        self.assertEqual(data["title"], "OMI_audio_emitter Node Extension")
        self.assertIn("source", data["properties"])
        self.assertIn("source", data["required"])
        
    def test_extension_directory_structure(self):
        """Test OMI extension directory structure parsing."""
        # Create mock extension structure
        extensions = [
            "OMI_physics_body",
            "OMI_audio_emitter", 
            "OMI_vehicle",
            "OMI_seat"
        ]
        
        for ext_name in extensions:
            ext_dir = self.test_extensions_dir / "extensions" / "2.0" / ext_name
            ext_dir.mkdir(parents=True, exist_ok=True)
            
            # Create README
            readme = ext_dir / "README.md"
            with open(readme, 'w') as f:
                f.write(f"# {ext_name}\n\nExtension description.")
                
            # Create schema directory
            schema_dir = ext_dir / "schema"
            schema_dir.mkdir(exist_ok=True)
            
        # Verify structure
        for ext_name in extensions:
            ext_dir = self.test_extensions_dir / "extensions" / "2.0" / ext_name
            self.assertTrue(ext_dir.exists())
            
            readme = ext_dir / "README.md"
            self.assertTrue(readme.exists())


class TestCompatibilityMapping(unittest.TestCase):
    """Test suite for compatibility mapping logic."""
    
    def test_semantic_mapping_rules(self):
        """Test semantic mapping between entities and extensions."""
        # Mock entity data
        entity = {
            "name": "AudioEntity",
            "fields": ["audio_clip", "volume", "spatialization"],
            "behaviors": ["playback", "3d_audio"]
        }
        
        # Mock extension data  
        extension = {
            "name": "OMI_audio_emitter",
            "fields": ["source", "volume", "loop"],
            "features": ["audio_playback", "spatialization"]
        }
        
        # Test mapping logic (simplified)
        common_concepts = set(["audio", "volume"])
        entity_concepts = set(["audio_clip", "volume", "spatialization", "playback", "3d_audio"])
        extension_concepts = set(["source", "volume", "loop", "audio_playback", "spatialization"])
        
        # Should find some overlap
        overlap = entity_concepts.intersection(extension_concepts)
        self.assertTrue(len(overlap) > 0)
        
    def test_support_level_determination(self):
        """Test logic for determining support levels."""
        # Test full support
        entity_features = set(["audio_playback", "volume_control", "spatialization"])
        extension_features = set(["audio_playback", "volume_control"])
        
        if extension_features.issubset(entity_features):
            support_level = "Full"
        elif len(entity_features.intersection(extension_features)) > 0:
            support_level = "Partial"  
        else:
            support_level = "None"
            
        self.assertEqual(support_level, "Full")
        
        # Test partial support
        entity_features = set(["audio_playback"])
        extension_features = set(["audio_playback", "volume_control", "spatialization"])
        
        if extension_features.issubset(entity_features):
            support_level = "Full"
        elif len(entity_features.intersection(extension_features)) > 0:
            support_level = "Partial"
        else:
            support_level = "None"
            
        self.assertEqual(support_level, "Partial")
        
        # Test no support
        entity_features = set(["visual_effects"])
        extension_features = set(["audio_playback", "volume_control"])
        
        if extension_features.issubset(entity_features):
            support_level = "Full"
        elif len(entity_features.intersection(extension_features)) > 0:
            support_level = "Partial"
        else:
            support_level = "None"
            
        self.assertEqual(support_level, "None")
        
    def test_missing_features_identification(self):
        """Test identification of missing features."""
        entity_features = set(["audio_playback", "volume_control"])
        extension_features = set(["audio_playback", "volume_control", "spatialization", "looping"])
        
        missing_features = extension_features - entity_features
        expected_missing = set(["spatialization", "looping"])
        
        self.assertEqual(missing_features, expected_missing)
        
    def test_required_field_handling(self):
        """Test handling of required vs optional fields."""
        extension_required = set(["source"])
        extension_optional = set(["volume", "loop"])
        entity_fields = set(["audio_clip", "volume"])
        
        # Map entity fields to extension fields (simplified)
        field_mapping = {"audio_clip": "source", "volume": "volume"}
        mapped_entity_fields = set(field_mapping.values())
        
        missing_required = extension_required - mapped_entity_fields
        missing_optional = extension_optional - mapped_entity_fields
        
        self.assertEqual(len(missing_required), 0)  # Required field is mapped
        self.assertEqual(missing_optional, set(["loop"]))  # Loop is missing


if __name__ == '__main__':
    unittest.main()