local didJustSpawnObj = false
local didJustSpawnObj2 = false


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
    
    if not addComponentsToCube(cube, material, true, true) then
        return
    end
end

function addComponentsToCube(cube, material, addRigid, setMesh)
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
    else
        print("Collider created successfully")
    end

    local rb = cube:AddComponent('Rigidbody')
    if not rb then
        print('Failed to create Rigidbody component')
        return false
    end

    if not addRigid then
        rb.IsKinematic = true
    end


    if setMesh ~= false then
        print('Creating cube mesh...')
        -- re-write this soon! (or not, whatever floats your boat)
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

    return true
end

function onUpdate(deltaTime)
    -- Update the camera position and rotation based on input
    if Input.IsKeyDown(Keys.W) then
        Camera.MoveForward(0.005)
    end
    if Input.IsKeyDown(Keys.S) then
        Camera.MoveForward(-0.005)
    end
    if Input.IsKeyDown(Keys.A) then
        Camera.MoveRight(-0.005)
    end
    if Input.IsKeyDown(Keys.D) then
        Camera.MoveRight(0.005)
    end
    if Input.IsKeyDown(Keys.Q) then
        Camera.MoveUp(-0.005)
    end
    if Input.IsKeyDown(Keys.E) then
        Camera.MoveUp(0.005)
    end
    if Input.IsKeyDown(Keys.Left) then
        Camera.RotateRight(-0.05)
    end
    if Input.IsKeyDown(Keys.Right) then
        Camera.RotateRight(0.05)
    end
    if Input.IsKeyDown(Keys.Up) then
        Camera.RotateUp(0.05)
    end
    if Input.IsKeyDown(Keys.Down) then
        Camera.RotateUp(-0.05)
    end
    if Input.IsKeyDown(Keys.Space) then
        if not didJustSpawnObj then
            print('Spawning new cube without Rigidbody...')
            local newCube = engine:CreateGameObject('Cube')
            newCube:SetPosition(0, 12, 0)

            local material = engine:CreateMaterial('default')
            if material then
                local texture = engine:LoadTexture("default")
                if texture then
                    material:SetTexture('diffuseTexture', texture)
                end
            end

            -- Add components: MeshRenderer, Collider (no Rigidbody)
            if addComponentsToCube(newCube, material, true, true) then
                print('New cube spawned successfully')
            else
                print('Failed to spawn new cube')
            end

            didJustSpawnObj = true
        end
    else
        didJustSpawnObj = false
    end
    if Input.IsKeyDown(Keys.L) then
        if not didJustSpawnObj2 then
            print('Spawning new cube without Rigidbody...')
            local newCube = engine:CreateGameObject('Cube')
            newCube:SetPosition(0, 9, 0)

            local material = engine:CreateMaterial('default')
            if material then
                local texture = engine:LoadTexture("default")
                if texture then
                    material:SetTexture('diffuseTexture', texture)
                end
            end

            -- Add components: MeshRenderer, Collider (no Rigidbody)
            if addComponentsToCube(newCube, material, false, true) then
                print('New cube spawned successfully')
            else
                print('Failed to spawn new cube')
            end

            didJustSpawnObj2 = true
        end
    else
        didJustSpawnObj2 = false
    end
end
