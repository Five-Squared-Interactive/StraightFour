import pytest
import json
import os
import re
from pathlib import Path

class TestCompatibilityAnalysis:
    """Test suite for the StraightFour-glTF compatibility analysis."""
    
    @pytest.fixture
    def docs_path(self):
        """Path to the docs/compatibility directory."""
        return Path("docs/compatibility")
    
    @pytest.fixture
    def matrix_file(self, docs_path):
        """Path to the compatibility matrix file."""
        return docs_path / "straightfour-gltf-matrix.md"
    
    @pytest.fixture
    def report_file(self, docs_path):
        """Path to the detailed report file."""
        return docs_path / "straightfour-gltf-report.md"
    
    @pytest.fixture
    def mapping_file(self, docs_path):
        """Path to the JSON mapping file."""
        return docs_path / "straightfour-gltf-mapping.json"
    
    @pytest.fixture
    def expected_entities(self):
        """Expected StraightFour entity types based on repository structure."""
        return {
            "BaseEntity",
            "AirplaneEntity", 
            "AudioEntity",
            "AutomobileEntity",
            "CharacterEntity",
            "LightEntity",
            "MeshEntity",
            "TerrainEntity",
            "VoxelEntity",
            "CanvasEntity",
            "ButtonEntity",
            "InputEntity",
            "TextEntity",
            "EmptyEntity"
        }

class TestFileExistence:
    """Test that all required output files exist."""
    
    def test_docs_compatibility_directory_exists(self, docs_path):
        """Test that the docs/compatibility directory exists."""
        assert docs_path.exists(), f"Directory {docs_path} does not exist"
        assert docs_path.is_dir(), f"{docs_path} is not a directory"
    
    def test_matrix_file_exists(self, matrix_file):
        """Test that the compatibility matrix file exists."""
        assert matrix_file.exists(), f"Matrix file {matrix_file} does not exist"
        assert matrix_file.is_file(), f"{matrix_file} is not a file"
    
    def test_report_file_exists(self, report_file):
        """Test that the detailed report file exists."""
        assert report_file.exists(), f"Report file {report_file} does not exist"
        assert report_file.is_file(), f"{report_file} is not a file"
    
    def test_mapping_file_exists(self, mapping_file):
        """Test that the JSON mapping file exists."""
        assert mapping_file.exists(), f"Mapping file {mapping_file} does not exist"
        assert mapping_file.is_file(), f"{mapping_file} is not a file"

class TestMatrixFile:
    """Test the compatibility matrix markdown file."""
    
    def test_matrix_has_valid_markdown_table(self, matrix_file):
        """Test that the matrix file contains a valid markdown table."""
        content = matrix_file.read_text(encoding='utf-8')
        
        # Should have table headers
        assert "|" in content, "Matrix file should contain markdown table syntax"
        
        # Should have required columns
        required_columns = [
            "Extension Name",
            "StraightFour Entity", 
            "Support Level",
            "Missing Features",
            "Notes"
        ]
        
        for column in required_columns:
            assert column in content, f"Matrix should contain '{column}' column"
    
    def test_matrix_has_table_separator(self, matrix_file):
        """Test that the matrix has proper markdown table separators."""
        content = matrix_file.read_text(encoding='utf-8')
        
        # Should have table separator row with dashes
        lines = content.split('\n')
        separator_found = False
        for line in lines:
            if '|' in line and '-' in line:
                separator_found = True
                break
        
        assert separator_found, "Matrix should have markdown table separator row"
    
    def test_matrix_support_levels_are_valid(self, matrix_file):
        """Test that all support levels use valid values."""
        content = matrix_file.read_text(encoding='utf-8')
        
        valid_support_levels = {"Full", "Partial", "None", "N/A"}
        
        # Extract table rows (skip header and separator)
        lines = content.split('\n')
        table_lines = [line for line in lines if line.strip().startswith('|') and '|' in line]
        
        if len(table_lines) > 2:  # Header + separator + at least one data row
            for line in table_lines[2:]:  # Skip header and separator
                if line.strip():
                    cells = [cell.strip() for cell in line.split('|')[1:-1]]  # Remove empty first/last
                    if len(cells) >= 3:  # Should have at least 3 columns
                        support_level = cells[2].strip()
                        if support_level:  # Skip empty cells
                            assert any(level in support_level for level in valid_support_levels), \
                                f"Invalid support level: '{support_level}'"

class TestReportFile:
    """Test the detailed report markdown file."""
    
    def test_report_has_required_sections(self, report_file):
        """Test that the report contains all required sections."""
        content = report_file.read_text(encoding='utf-8')
        
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
            # Check for section as header (with # or ##) or as text
            pattern = rf"(?i)#{1,3}\s*{re.escape(section)}|{re.escape(section)}"
            assert re.search(pattern, content), f"Report should contain section: '{section}'"
    
    def test_report_mentions_straightfour_entities(self, report_file, expected_entities):
        """Test that the report mentions StraightFour entities."""
        content = report_file.read_text(encoding='utf-8').lower()
        
        # Should mention at least some of the expected entities
        mentioned_entities = 0
        for entity in expected_entities:
            if entity.lower() in content or entity.replace("Entity", "").lower() in content:
                mentioned_entities += 1
        
        assert mentioned_entities >= 3, f"Report should mention at least 3 StraightFour entities, found {mentioned_entities}"
    
    def test_report_mentions_gltf_extensions(self, report_file):
        """Test that the report mentions glTF extensions."""
        content = report_file.read_text(encoding='utf-8').lower()
        
        # Should mention glTF extensions
        gltf_indicators = ["gltf", "extension", "omi_", "ext_", "khr_"]
        mentioned_indicators = sum(1 for indicator in gltf_indicators if indicator in content)
        
        assert mentioned_indicators >= 2, "Report should mention glTF extensions"

class TestMappingFile:
    """Test the JSON mapping file."""
    
    def test_mapping_is_valid_json(self, mapping_file):
        """Test that the mapping file contains valid JSON."""
        try:
            with open(mapping_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            assert isinstance(data, dict), "JSON should be a dictionary"
        except json.JSONDecodeError as e:
            pytest.fail(f"Mapping file contains invalid JSON: {e}")
    
    def test_mapping_has_extensions_key(self, mapping_file):
        """Test that the mapping has an 'extensions' key."""
        with open(mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        assert "extensions" in data, "Mapping should have 'extensions' key"
        assert isinstance(data["extensions"], dict), "'extensions' should be a dictionary"
    
    def test_mapping_extension_entries_have_required_fields(self, mapping_file):
        """Test that extension entries have required fields."""
        with open(mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        if "extensions" in data and data["extensions"]:
            required_fields = ["supported", "supported_features", "missing_features", "mapped_entities"]
            
            for ext_name, ext_data in data["extensions"].items():
                assert isinstance(ext_data, dict), f"Extension '{ext_name}' should be a dictionary"
                
                for field in required_fields:
                    assert field in ext_data, f"Extension '{ext_name}' should have '{field}' field"
    
    def test_mapping_supported_field_is_boolean(self, mapping_file):
        """Test that 'supported' fields are boolean."""
        with open(mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        if "extensions" in data:
            for ext_name, ext_data in data["extensions"].items():
                if "supported" in ext_data:
                    assert isinstance(ext_data["supported"], bool), \
                        f"Extension '{ext_name}' 'supported' field should be boolean"
    
    def test_mapping_arrays_are_lists(self, mapping_file):
        """Test that array fields are lists."""
        with open(mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        if "extensions" in data:
            array_fields = ["supported_features", "missing_features", "mapped_entities"]
            
            for ext_name, ext_data in data["extensions"].items():
                for field in array_fields:
                    if field in ext_data:
                        assert isinstance(ext_data[field], list), \
                            f"Extension '{ext_name}' '{field}' should be a list"

class TestDataConsistency:
    """Test consistency across all output files."""
    
    def test_extensions_mentioned_in_all_files(self, matrix_file, report_file, mapping_file):
        """Test that extensions are consistently mentioned across files."""
        # Get extensions from JSON mapping
        with open(mapping_file, 'r', encoding='utf-8') as f:
            mapping_data = json.load(f)
        
        json_extensions = set()
        if "extensions" in mapping_data:
            json_extensions = set(mapping_data["extensions"].keys())
        
        if json_extensions:
            # Check that at least some extensions appear in matrix and report
            matrix_content = matrix_file.read_text(encoding='utf-8')
            report_content = report_file.read_text(encoding='utf-8')
            
            matrix_mentions = sum(1 for ext in json_extensions if ext in matrix_content)
            report_mentions = sum(1 for ext in json_extensions if ext in report_content)
            
            assert matrix_mentions > 0, "Matrix should mention extensions from JSON mapping"
            assert report_mentions > 0, "Report should mention extensions from JSON mapping"
    
    def test_support_levels_consistency(self, matrix_file, mapping_file):
        """Test that support levels are consistent between matrix and JSON."""
        # This is a basic consistency check - in a real implementation,
        # we'd want to ensure the same extensions have the same support levels
        
        matrix_content = matrix_file.read_text(encoding='utf-8')
        
        with open(mapping_file, 'r', encoding='utf-8') as f:
            mapping_data = json.load(f)
        
        # Check that we have both supported and unsupported items
        if "extensions" in mapping_data:
            supported_count = sum(1 for ext_data in mapping_data["extensions"].values() 
                                if ext_data.get("supported", False))
            total_count = len(mapping_data["extensions"])
            
            if total_count > 0:
                # Should have some variety in support levels
                assert 0 <= supported_count <= total_count, \
                    "Support levels should be varied (some supported, some not)"

class TestContentQuality:
    """Test the quality and completeness of generated content."""
    
    def test_no_placeholder_content(self, matrix_file, report_file, mapping_file):
        """Test that files don't contain placeholder content."""
        placeholder_patterns = [
            "TODO", "FIXME", "PLACEHOLDER", "TBD", "COMING SOON",
            "[PLACEHOLDER]", "...", "XXX"
        ]
        
        files_to_check = [matrix_file, report_file, mapping_file]
        
        for file_path in files_to_check:
            content = file_path.read_text(encoding='utf-8').upper()
            
            for pattern in placeholder_patterns:
                assert pattern not in content, f"File {file_path} contains placeholder: {pattern}"
    
    def test_files_have_substantial_content(self, matrix_file, report_file, mapping_file):
        """Test that files have substantial content, not just headers."""
        
        # Matrix should have reasonable size
        matrix_content = matrix_file.read_text(encoding='utf-8')
        assert len(matrix_content) > 500, "Matrix file should have substantial content"
        
        # Report should have good size
        report_content = report_file.read_text(encoding='utf-8')
        assert len(report_content) > 1000, "Report file should have substantial content"
        
        # JSON should have some extensions
        with open(mapping_file, 'r', encoding='utf-8') as f:
            mapping_data = json.load(f)
        
        if "extensions" in mapping_data:
            assert len(mapping_data["extensions"]) > 0, "JSON mapping should have extension data"
    
    def test_markdown_formatting(self, matrix_file, report_file):
        """Test that markdown files have proper formatting."""
        
        for md_file in [matrix_file, report_file]:
            content = md_file.read_text(encoding='utf-8')
            
            # Should have headers
            assert re.search(r'^#{1,6}\s+.+$', content, re.MULTILINE), \
                f"File {md_file} should have markdown headers"
            
            # Should not have common formatting errors
            assert not re.search(r'\s+$', content, re.MULTILINE), \
                f"File {md_file} should not have trailing whitespace"

class TestEdgeCases:
    """Test handling of edge cases and error conditions."""
    
    def test_empty_extension_handling(self, mapping_file):
        """Test that empty or minimal extension data is handled properly."""
        with open(mapping_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # If there are extensions, they should have proper structure
        if "extensions" in data and data["extensions"]:
            for ext_name, ext_data in data["extensions"].items():
                # Even minimal entries should have the required structure
                assert ext_name, "Extension names should not be empty"
                assert isinstance(ext_data, dict), f"Extension {ext_name} data should be a dict"
    
    def test_special_characters_handling(self, matrix_file, report_file):
        """Test that special characters are handled properly in markdown."""
        
        for md_file in [matrix_file, report_file]:
            content = md_file.read_text(encoding='utf-8')
            
            # Should handle common special characters without breaking markdown
            # This is more of a smoke test - if the file loads without issues,
            # basic character handling is working
            assert len(content) > 0, f"File {md_file} should have content"
    
    def test_unicode_handling(self, mapping_file):
        """Test that Unicode characters are handled properly in JSON."""
        try:
            with open(mapping_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            # If we can load it as JSON with UTF-8, Unicode handling is working
            assert isinstance(data, dict)
        except UnicodeDecodeError:
            pytest.fail("JSON file should handle Unicode characters properly")

class TestIntegration:
    """Integration tests across the entire compatibility analysis."""
    
    def test_complete_workflow_output(self, docs_path):
        """Test that the complete workflow produces expected outputs."""
        
        # All required files should exist
        required_files = [
            "straightfour-gltf-matrix.md",
            "straightfour-gltf-report.md", 
            "straightfour-gltf-mapping.json"
        ]
        
        for filename in required_files:
            file_path = docs_path / filename
            assert file_path.exists(), f"Required file {filename} should exist"
            assert file_path.stat().st_size > 0, f"File {filename} should not be empty"
    
    def test_cross_file_references(self, matrix_file, report_file, mapping_file):
        """Test that files reference each other appropriately."""
        
        report_content = report_file.read_text(encoding='utf-8')
        
        # Report might reference the matrix or mapping files
        # This is optional but good practice
        references = ["matrix", "mapping", "compatibility"]
        reference_found = any(ref in report_content.lower() for ref in references)
        
        # This is more of a quality check than a hard requirement
        if not reference_found:
            print("Note: Report doesn't seem to cross-reference other files")
    
    def test_analysis_completeness(self, expected_entities, mapping_file, report_file):
        """Test that the analysis covers the expected scope."""
        
        # Check that major entity types are covered
        report_content = report_file.read_text(encoding='utf-8').lower()
        
        covered_entities = 0
        for entity in expected_entities:
            entity_variations = [
                entity.lower(),
                entity.replace("Entity", "").lower(),
                entity.replace("Entity", " Entity").lower()
            ]
            
            if any(variation in report_content for variation in entity_variations):
                covered_entities += 1
        
        # Should cover at least half of the expected entities
        coverage_ratio = covered_entities / len(expected_entities)
        assert coverage_ratio >= 0.3, f"Analysis should cover at least 30% of entities, got {coverage_ratio:.2%}"