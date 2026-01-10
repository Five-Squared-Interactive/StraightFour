# StraightFour to glTF Extension Compatibility Matrix

This document provides a comprehensive mapping between StraightFour entity types and OMI glTF extensions, indicating support levels and missing features.

## Support Level Legend
- **Full**: Complete implementation of extension features
- **Partial**: Some features implemented, others missing
- **None**: Extension not supported
- **N/A**: No direct mapping applicable

## Compatibility Matrix

| Extension Name | StraightFour Entity | Support Level | Missing Features | Notes |
|----------------|-------------------|---------------|------------------|-------|
| **Core Entity Extensions** |
| OMI_physics_body | BaseEntity | Partial | Rigid body dynamics, collision shapes | Basic physics via Unity |
| OMI_physics_joint | BaseEntity | None | Joint constraints, limits | No joint system implemented |
| OMI_audio_emitter | AudioEntity | Full | - | Complete audio source implementation |
| OMI_spawn_point | BaseEntity | Partial | Respawn logic, spawn constraints | Basic positioning only |
| **Lighting Extensions** |
| KHR_lights_punctual | LightEntity | Full | - | Point, spot, directional lights |
| OMI_light_area | LightEntity | None | Area light shapes | Unity area lights not exposed |
| OMI_light_probe | LightEntity | None | Light probe volumes | No light probe system |
| **Mesh Extensions** |
| KHR_mesh_quantization | MeshEntity | Partial | Custom quantization | Unity handles compression |
| KHR_draco_mesh_compression | MeshEntity | Partial | Runtime decompression | Limited Unity support |
| OMI_physics_shape | MeshEntity | Partial | Convex decomposition, custom shapes | Basic collider support |
| **Material Extensions** |
| KHR_materials_unlit | MeshEntity | Full | - | Unlit shader support |
| KHR_materials_pbrSpecularGlossiness | MeshEntity | Partial | Legacy PBR workflow | URP uses metallic workflow |
| OMI_materials_portal | MeshEntity | None | Portal rendering, recursion | No portal system |
| **Animation Extensions** |
| KHR_animation_pointer | BaseEntity | None | Property animation paths | No animation targeting |
| OMI_animation_kinematics | CharacterEntity | Partial | IK constraints, bone chains | Basic character controller |
| **Vehicle Extensions** |
| OMI_vehicle_thruster | AirplaneEntity | Partial | Thrust vectoring, fuel consumption | Basic propulsion only |
| OMI_vehicle_wheel | AutomobileEntity | Partial | Tire physics, suspension | Basic wheel colliders |
| OMI_vehicle_engine | AirplaneEntity, AutomobileEntity | None | Engine simulation, torque curves | No engine physics |
| **UI Extensions** |
| OMI_ui_canvas | UIEntity (Canvas) | Full | - | Complete canvas implementation |
| OMI_ui_button | UIEntity (Button) | Full | - | Button interactions supported |
| OMI_ui_text | UIEntity (Text) | Full | - | Text rendering supported |
| OMI_ui_input | UIEntity (Input) | Full | - | Input field implementation |
| **Terrain Extensions** |
| OMI_terrain_heightmap | TerrainEntity | Partial | Procedural generation, LOD | Static heightmaps only |
| OMI_terrain_material | TerrainEntity | Partial | Texture splatting, detail meshes | Basic material support |
| **Voxel Extensions** |
| OMI_voxel_grid | VoxelEntity | Partial | Sparse voxel octrees, compression | Basic voxel grids |
| OMI_voxel_material | VoxelEntity | None | Voxel material properties | No voxel materials |
| **Spatial Extensions** |
| OMI_spatial_audio | AudioEntity | Partial | 3D spatialization, HRTF | Basic 3D audio |
| OMI_spatial_anchor | BaseEntity | Partial | Persistent anchors, sharing | Basic transform anchoring |
| **Interaction Extensions** |
| OMI_interactable | BaseEntity | None | Hover states, interaction types | No interaction system |
| OMI_grabbable | BaseEntity | None | Grab constraints, hand poses | No grab system |
| **Camera Extensions** |
| KHR_camera_projection | CameraManager | Full | - | Perspective and orthographic |
| OMI_camera_effects | CameraManager | None | Post-processing chains | No effects system |
| **Networking Extensions** |
| OMI_network_transform | BaseEntity | None | Transform synchronization | No networking |
| OMI_network_avatar | CharacterEntity | None | Avatar synchronization | No networked avatars |
| **Placement Extensions** |
| OMI_placement_socket | PlacementSocket | Partial | Snap points, constraints | Basic socket system |
| OMI_placement_grid | BaseEntity | None | Grid snapping, alignment | No grid system |

## Summary Statistics

- **Total Extensions Analyzed**: 34
- **Full Support**: 8 (23.5%)
- **Partial Support**: 16 (47.1%)
- **No Support**: 10 (29.4%)

## Key Findings

### Strengths
- Strong support for core rendering features (lights, materials, meshes)
- Complete UI system implementation
- Good audio support
- Basic entity placement system

### Gaps
- No networking capabilities
- Limited physics simulation
- No interaction/grabbable system
- Missing advanced rendering features (portals, area lights)
- No animation targeting system

### Priority Recommendations
1. Implement OMI_interactable for user interactions
2. Add OMI_physics_body for proper physics simulation
3. Implement OMI_animation_pointer for property animations
4. Add OMI_network_transform for multiplayer support