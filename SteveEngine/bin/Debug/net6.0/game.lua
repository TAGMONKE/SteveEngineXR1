function onStart()
    print('SteveEngine game started!')

    local cube = engine:CreateGameObject('Cube')
    cube:SetPosition(0, 0, 0)

    print('Creating material...')
    local material = engine:CreateMaterial('default')
    if not material then
        print('Failed to create material')
        return
    end

    local texture = engine:LoadTexture("default")
    if texture then
        print('Texture loaded, setting on material')
        material:SetTexture('diffuseTexture', texture)
    else
        print('Failed to load texture')
    end

    if not addComponentsToCube(cube, material) then
        return
    end
end

function addComponentsToCube(cube, material, setMesh)
    print('Adding components to cube...')
    local meshRenderer = cube:AddComponent('MeshRenderer')
    if not meshRenderer then
        print('Failed to create MeshRenderer component')
        return false
    end

    local col = cube:AddComponent('Collider')
    if not col then
        print('Failed to create Collider component')
        return false
    end

    local rb = cube:AddComponent('Rigidbody')
    if not rb then
        print('Failed to create Rigidbody component')
        return false
    end


    if setMesh ~= false then
        print('Creating cube mesh...')
        local cubeMesh = Mesh.CreateCube()
        if cubeMesh then
            print('Cube mesh created successfully')
            meshRenderer.Mesh = cubeMesh
            meshRenderer.Material = material
        else
            print('Failed to create cube mesh')
            return false
        end
    end

    rb:AddForceXYZ(0, 10, 0, 0)

    return true
end

function onUpdate(deltaTime)
    local count = GetGameObjectCount()
    if count > 0 then
        local cube = GetGameObject(1)
        if cube and cube.Transform and cube.Transform.Rotation then
            cube.Transform.Rotation.X = cube.Transform.Rotation.X + deltaTime
            cube.Transform.Rotation.Y = cube.Transform.Rotation.Y + deltaTime * 2
        end
    end

    if Input.IsKeyDown(Keys.W) then
        print("hello, world!")
    end

end
