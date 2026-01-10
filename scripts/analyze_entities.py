#!/usr/bin/env python3
"""
Analyze StraightFour entities by parsing C# files and extracting metadata.
"""

import os
import re
import json
import logging
from pathlib import Path
from typing import Dict, List, Any, Optional

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class StraightFourEntityAnalyzer:
    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.assets_path = self.project_root / "Assets" / "StraightFour"
        self.entity_catalog = {}
        
    def scan_entity_directories(self) -> List[Path]:
        """Scan for entity-related directories and C# files."""
        entity_files = []
        
        if not self.assets_path.exists():
            logger.warning(f"Assets path not found: {self.assets_path}")
            return entity_files
            
        # Look for .cs files in the StraightFour directory
        for cs_file in self.assets_path.rglob("*.cs"):
            if self._is_entity_file(cs_file):
                entity_files.append(cs_file)
                logger.info(f"Found entity file: {cs_file.relative_to(self.project_root)}")
                
        return entity_files
    
    def _is_entity_file(self, file_path: Path) -> bool:
        """Check if a C# file appears to be an entity definition."""
        # Check if filename contains "Entity" or is in Entity directory
        if "Entity" in str(file_path) or "entity" in file_path.name.lower():
            return True
        
        # Check file contents for entity-like patterns
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
                if re.search(r'class\s+\w*Entity\w*', content, re.IGNORECASE):
                    return True
                if re.search(r':\s*(MonoBehaviour|BaseEntity)', content):
                    return True
        except Exception as e:
            logger.warning(f"Error reading file {file_path}: {e}")
            
        return False
    
    def parse_cs_files(self, entity_files: List[Path]) -> Dict[str, Any]:
        """Parse C# entity files and extract metadata."""
        entities = {}
        
        for file_path in entity_files:
            try:
                entity_info = self._parse_single_cs_file(file_path)
                if entity_info:
                    entities[entity_info['name']] = entity_info
            except Exception as e:
                logger.error(f"Error parsing {file_path}: {e}")
                
        return entities
    
    def _parse_single_cs_file(self, file_path: Path) -> Optional[Dict[str, Any]]:
        """Parse a single C# file and extract entity information."""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
        except Exception as e:
            logger.error(f"Could not read file {file_path}: {e}")
            return None
            
        entity_info = {
            'name': file_path.stem,
            'file_path': str(file_path.relative_to(self.project_root)),
            'namespace': self._extract_namespace(content),
            'class_name': self._extract_class_name(content),
            'base_class': self._extract_base_class(content),
            'properties': self._extract_properties(content),
            'methods': self._extract_methods(content),
            'description': self._extract_description(content),
            'category': self._determine_category(file_path),
            'unity_components': self._extract_unity_components(content)
        }
        
        return entity_info
    
    def _extract_namespace(self, content: str) -> Optional[str]:
        """Extract namespace from C# content."""
        match = re.search(r'namespace\s+([^\s{]+)', content)
        return match.group(1) if match else None
    
    def _extract_class_name(self, content: str) -> Optional[str]:
        """Extract main class name from C# content."""
        match = re.search(r'public\s+class\s+(\w+)', content)
        return match.group(1) if match else None
    
    def _extract_base_class(self, content: str) -> Optional[str]:
        """Extract base class name."""
        match = re.search(r'class\s+\w+\s*:\s*(\w+)', content)
        return match.group(1) if match else None
    
    def _extract_properties(self, content: str) -> List[Dict[str, str]]:
        """Extract public properties and fields."""
        properties = []
        
        # Find SerializeField attributes and public fields
        serialize_field_pattern = r'\[SerializeField\]\s*(?:private\s+|protected\s+)?(\w+)\s+(\w+)'
        public_field_pattern = r'public\s+(\w+)\s+(\w+)(?:\s*[=;])'
        property_pattern = r'public\s+(\w+)\s+(\w+)\s*\{\s*get'
        
        for pattern in [serialize_field_pattern, public_field_pattern, property_pattern]:
            for match in re.finditer(pattern, content):
                properties.append({
                    'type': match.group(1),
                    'name': match.group(2),
                    'access': 'public' if 'public' in pattern else 'private'
                })
                
        return properties
    
    def _extract_methods(self, content: str) -> List[Dict[str, str]]:
        """Extract public methods."""
        methods = []
        method_pattern = r'public\s+(?:virtual\s+|override\s+)?(\w+)\s+(\w+)\s*\('
        
        for match in re.finditer(method_pattern, content):
            methods.append({
                'return_type': match.group(1),
                'name': match.group(2)
            })
            
        return methods
    
    def _extract_description(self, content: str) -> Optional[str]:
        """Extract description from comments."""
        # Look for XML documentation comments
        xml_doc_pattern = r'///\s*<summary>(.*?)</summary>'
        match = re.search(xml_doc_pattern, content, re.DOTALL)
        if match:
            return match.group(1).strip()
            
        # Look for regular comments at the top of the class
        class_comment_pattern = r'/\*\*(.*?)\*/\s*public\s+class'
        match = re.search(class_comment_pattern, content, re.DOTALL)
        if match:
            return match.group(1).strip()
            
        return None
    
    def _determine_category(self, file_path: Path) -> str:
        """Determine entity category based on file path."""
        path_str = str(file_path).lower()
        
        if 'airplane' in path_str:
            return 'vehicle'
        elif 'automobile' in path_str:
            return 'vehicle'
        elif 'audio' in path_str:
            return 'audio'
        elif 'camera' in path_str:
            return 'camera'
        elif 'base' in path_str:
            return 'base'
        elif 'ui' in path_str:
            return 'ui'
        elif 'light' in path_str:
            return 'light'
        elif 'mesh' in path_str:
            return 'mesh'
        elif 'terrain' in path_str:
            return 'terrain'
        elif 'character' in path_str:
            return 'character'
        else:
            return 'unknown'
    
    def _extract_unity_components(self, content: str) -> List[str]:
        """Extract Unity component requirements."""
        components = []
        
        # Look for RequireComponent attributes
        require_pattern = r'\[RequireComponent\(typeof\((\w+)\)\)\]'
        for match in re.finditer(require_pattern, content):
            components.append(match.group(1))
            
        # Look for GetComponent calls
        getcomponent_pattern = r'GetComponent<(\w+)>\(\)'
        for match in re.finditer(getcomponent_pattern, content):
            if match.group(1) not in components:
                components.append(match.group(1))
                
        return components
    
    def generate_entity_catalog(self) -> Dict[str, Any]:
        """Generate complete entity catalog."""
        logger.info("Starting StraightFour entity analysis...")
        
        entity_files = self.scan_entity_directories()
        entities = self.parse_cs_files(entity_files)
        
        catalog = {
            'metadata': {
                'total_entities': len(entities),
                'categories': list(set(e['category'] for e in entities.values())),
                'analysis_timestamp': self._get_timestamp()
            },
            'entities': entities
        }
        
        self.entity_catalog = catalog
        logger.info(f"Analyzed {len(entities)} entities")
        return catalog
    
    def _get_timestamp(self) -> str:
        """Get current timestamp."""
        from datetime import datetime
        return datetime.now().isoformat()
    
    def save_catalog(self, output_path: str) -> None:
        """Save entity catalog to JSON file."""
        with open(output_path, 'w') as f:
            json.dump(self.entity_catalog, f, indent=2)
        logger.info(f"Entity catalog saved to {output_path}")

def main():
    import argparse
    
    parser = argparse.ArgumentParser(description='Analyze StraightFour entities')
    parser.add_argument('--project-root', default='.', help='Path to StraightFour project root')
    parser.add_argument('--output', default='entity_catalog.json', help='Output file for entity catalog')
    
    args = parser.parse_args()
    
    analyzer = StraightFourEntityAnalyzer(args.project_root)
    catalog = analyzer.generate_entity_catalog()
    analyzer.save_catalog(args.output)
    
    print(f"Found {catalog['metadata']['total_entities']} entities")
    print(f"Categories: {', '.join(catalog['metadata']['categories'])}")

if __name__ == '__main__':
    main()