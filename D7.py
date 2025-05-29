

def add(*nums):
    sum1 = 0
    for num in nums:
        sum1 += num
    return sum1

def sub(*nums):
    sum2 = nums[0]
    for num in nums[1:]:
        sum2 -= num
    return sum2

def mult(*nums):
    sum3 = nums[0]
    for num in nums[1:]:
        sum3 *= num
    return sum3
        
a = int(input("First Number: "))
b = int(input("Second Number: "))

print(add(a,b))
print(sub(a,b))

