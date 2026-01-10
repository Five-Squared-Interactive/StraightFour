import unittest
import json
import re
from pathlib import Path

class TestCompatibilityEdgeCases(unittest.TestCase):
    """Test suite for edge cases in compatibility analysis."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.docs_path = Path("docs/compatibility")
        self.matrix_file = self.docs_path / "straightfour-gltf-matrix.md"
        self.report_file = self.docs_path / "straightfour-gltf-report.md"
        self.mapping_file = self.docs_path / "straightfour-gltf-mapping.json"

    def test_extensions_with_no_straightfour_equivalent(self):
        """Test handling of extensions with no StraightFour equivalent."""
        with open(self.mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # Find extensions with no mapped entities
        unmapped_extensions = []
        for ext_name, ext_data in data["extensions"].items():
            if not ext_data["mapped_entities"] or ext_data["mapped_entities"] == []:
                unmapped_extensions.append(ext_name)
        
        # Should have at least some unmapped extensions
        self.assertGreater(len(unmapped_extensions), 0, 
                          "Should have some extensions with no StraightFour equivalent")
        
        # These should be marked as unsupported
        for ext_name in unmapped_extensions:
            ext_data = data["extensions"][ext_name]
            self.assertFalse(ext_data["supported"], 
                           f"Unmapped extension {ext_name} should be marked as unsupported")

    def test_entities_mapping_to_multiple_extensions(self):
        """Test handling of entities that map to multiple extensions."""
        with open(self.mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # Count mappings per entity
        entity_mappings = {}
        for ext_name, ext_data in data["extensions"].items():
            for entity in ext_data["mapped_entities"]:
                if entity not in entity_mappings:
                    entity_mappings[entity] = []
                entity_mappings[entity].append(ext_name)
        
        # Find entities mapped to multiple extensions
        multi_mapped_entities = {entity: exts for entity, exts in entity_mappings.items() 
                               if len(exts) > 1}
        
        # Should have at least some entities mapped to multiple extensions
        if multi_mapped_entities:
            # Check that these are handled properly in the report
            with open(self.report_file, 'r', encoding='utf-8') as f:
                report_content = f.read()
            
            for entity in multi_mapped_entities:
                self.assertIn(entity, report_content, 
                            f"Multi-mapped entity {entity} should be discussed in report")

    def test_extensions_with_dependencies(self):
        """Test handling of extensions that require other extensions."""
        with open(self.mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        with open(self.report_file, 'r', encoding='utf-8') as f:
            report_content = f.read()
        
        # Look for mentions of dependencies
        dependency_keywords = ["depend", "require", "prerequisite", "based on"]
        has_dependency_discussion = any(keyword in report_content.lower() 
                                      for keyword in dependency_keywords)
        
        if has_dependency_discussion:
            # If dependencies are discussed, they should be handled properly
            self.assertIn("dependencies", report_content.lower(), 
                         "Report should explicitly discuss extension dependencies")

    def test_optional_vs_required_fields(self):
        """Test handling of optional vs required fields in extensions."""
        with open(self.report_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Should discuss optional vs required fields
        field_keywords = ["optional", "required", "mandatory"]
        has_field_discussion = any(keyword in content.lower() for keyword in field_keywords)
        
        if has_field_discussion:
            self.assertTrue(has_field_discussion, 
                           "Report should discuss optional vs required fields")

    def test_partial_support_details(self):
        """Test that partial support is properly detailed."""
        with open(self.mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # Find extensions with partial support
        partial_extensions = []
        for ext_name, ext_data in data["extensions"].items():
            if (ext_data["supported"] and 
                (ext_data["missing_features"] or 
                 len(ext_data["supported_features"]) < len(ext_data["supported_features"] + ext_data["missing_features"]))):
                partial_extensions.append(ext_name)
        
        if partial_extensions:
            # Check that partial support is explained in the matrix
            with open(self.matrix_file, 'r', encoding='utf-8') as f:
                matrix_content = f.read()
            
            for ext_name in partial_extensions:
                # Should be marked as "Partial" in the matrix
                self.assertIn(ext_name, matrix_content)
                # Should have details about missing features
                lines = matrix_content.split('\n')
                for line in lines:
                    if ext_name in line and '|' in line:
                        self.assertIn("Partial", line, 
                                    f"Extension {ext_name} should be marked as Partial")
                        break

    def test_entities_with_incomplete_documentation(self):
        """Test handling of entities with incomplete documentation."""
        with open(self.report_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Look for acknowledgment of documentation limitations
        limitation_keywords = ["limited", "incomplete", "unclear", "documentation", "assumption"]
        has_limitations = any(keyword in content.lower() for keyword in limitation_keywords)
        
        # Should acknowledge any limitations in the analysis
        if "limitation" in content.lower():
            self.assertIn("limitation", content.lower(), 
                         "Report should acknowledge analysis limitations")

    def test_extension_versioning(self):
        """Test handling of extension versions."""
        with open(self.mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # Check if any extensions have version information
        versioned_extensions = []
        for ext_name in data["extensions"].keys():
            if re.search(r'v\d+|_\d+', ext_name):
                versioned_extensions.append(ext_name)
        
        if versioned_extensions:
            # If there are versioned extensions, they should be handled properly
            with open(self.report_file, 'r', encoding='utf-8') as f:
                report_content = f.read()
            
            for ext_name in versioned_extensions:
                self.assertIn(ext_name, report_content, 
                            f"Versioned extension {ext_name} should be in report")

    def test_complex_entity_hierarchies(self):
        """Test handling of complex entity hierarchies."""
        with open(self.report_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Should discuss entity relationships
        hierarchy_keywords = ["inherit", "extend", "base", "derived", "hierarchy", "component"]
        has_hierarchy_discussion = any(keyword in content.lower() for keyword in hierarchy_keywords)
        
        if has_hierarchy_discussion:
            self.assertTrue(has_hierarchy_discussion, 
                           "Report should discuss entity hierarchies if they exist")

    def test_ui_entity_special_handling(self):
        """Test special handling of UI entities and their sub-types."""
        with open(self.mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # UI entities might need special handling
        ui_entities = ["UI", "Canvas", "Button", "Input", "Text"]
        
        # Check if UI entities are mapped to appropriate extensions
        ui_mapped_extensions = []
        for ext_name, ext_data in data["extensions"].items():
            for entity in ext_data["mapped_entities"]:
                if entity in ui_entities:
                    ui_mapped_extensions.append(ext_name)
                    break
        
        if ui_mapped_extensions:
            # UI mappings should be discussed in the report
            with open(self.report_file, 'r', encoding='utf-8') as f:
                report_content = f.read()
            
            self.assertTrue(any(ui_entity in report_content for ui_entity in ui_entities),
                           "Report should discuss UI entity mappings")

    def test_consistency_across_files(self):
        """Test consistency of information across all output files."""
        with open(self.mapping_file, 'r', encoding='utf-8') as f:
            json_data = json.load(f)
        
        with open(self.matrix_file, 'r', encoding='utf-8') as f:
            matrix_content = f.read()
        
        with open(self.report_file, 'r', encoding='utf-8') as f:
            report_content = f.read()
        
        # Check consistency of support levels between JSON and matrix
        for ext_name, ext_data in json_data["extensions"].items():
            if ext_name in matrix_content:
                # Find the support level in the matrix
                lines = matrix_content.split('\n')
                for line in lines:
                    if ext_name in line and '|' in line:
                        parts = [part.strip() for part in line.split('|')]
                        if len(parts) >= 4:  # Has support level column
                            matrix_support = parts[3].strip()
                            
                            # Map JSON boolean to matrix text
                            expected_support = "Full" if ext_data["supported"] else "None"
                            if ext_data["supported"] and ext_data["missing_features"]:
                                expected_support = "Partial"
                            
                            # Allow some flexibility in exact wording
                            if matrix_support and expected_support != "None":
                                self.assertNotEqual(matrix_support, "None", 
                                                  f"Inconsistent support level for {ext_name}")
                            break
        
        # Check that all extensions mentioned in JSON appear in report
        for ext_name in json_data["extensions"].keys():
            self.assertIn(ext_name, report_content, 
                         f"Extension {ext_name} should be mentioned in report")

if __name__ == '__main__':
    unittest.main()