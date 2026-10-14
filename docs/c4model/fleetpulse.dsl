workspace "FleetPulse" "FleetPulse Distribuited system"

    !identifiers hierarchical

    model {
        u = person "Operator"
        simulator = softwareSystem "GPS Simulator" 
        
        ss = softwareSystem "Software System" {
            
            spa = container  "SPA root"
            mfealert = container  "Alerts MFE"
            mfedriver = container  "Drivers MFE"
            
            signalrhub = container "SignalR Hub"
            api = container "Api" {
                root = component "API root"   
                alerts = component "Alert Module"
                drivers = component "Drivers Module"
                
                root -> alerts "use"
                root -> drivers "use"
                
                
            }
            dbwriter = container "DB Writer"
            aiworker = container "AI worker"
            
            redpanda = container "RedPanda"
            db = container "Database Hypertable" {
                tags "Database"
            }
            
            api.drivers -> db "use"
            api.alerts -> db "use"
            
            mfealert -> api "uses"
            mfedriver -> api "uses"
            
            api -> db "Reads from and writes to"
            
            spa -> mfealert "uses"
            spa -> mfedriver "uses"
            
            signalrhub -> spa "sends"
            
            
            redpanda -> signalrhub "broadcast gps"
            redpanda -> aiworker "broadcast gps"
            redpanda -> dbwriter "broadcast gps"
            
            aiworker -> redpanda "publish alert"
            
            dbwriter -> db "writes"
            
        }
        simulator -> ss.redpanda "sends gps"
        u -> ss.spa "Uses"
        
        
    }

    views {
        systemContext ss "Diagram1" {
            include *
        }

        container ss "Diagram2" {
            include *
        }
        
        component ss.api "Diagram3"{
            include *
        }
    

        styles {
            element "Element" {
                color #0773af
                stroke #0773af
                strokeWidth 7
                shape roundedbox
            }
            element "Person" {
                shape person
            }
            element "Database" {
                shape cylinder
            }
            element "Boundary" {
                strokeWidth 5
            }
            relationship "Relationship" {
                thickness 4
            }
        }
    }

}